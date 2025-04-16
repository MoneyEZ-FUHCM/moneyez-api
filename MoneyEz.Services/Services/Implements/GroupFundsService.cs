using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupMember;
using MoneyEz.Services.Services.Interfaces;
using AutoMapper;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;
using Microsoft.AspNetCore.Http;
using MoneyEz.Services.Constants;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.Utils;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.GroupFund;
using System.Web;
using MoneyEz.Services.BusinessModels.EmailModels;
using MoneyEz.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.GroupFund.GroupInvite;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.BusinessModels.ImageModels;
using MoneyEz.Repositories.Commons.Filters;
using Microsoft.AspNetCore.Http.HttpResults;
using MoneyEz.Services.BusinessModels.GroupFundLogModels;
using MoneyEz.Services.BusinessModels.FinancialReportModels;
using MoneyEz.Services.BusinessModels.GroupMemLogModels;
using MoneyEz.Services.BusinessModels.TransactionModels.Group;
using MoneyEz.Services.Utils.Email;
using System.Text.RegularExpressions;
namespace MoneyEz.Services.Services.Implements
{
    public class GroupFundsService : IGroupFundsService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly IRedisService _redisService;
        private readonly IMailService _mailService;
        private readonly INotificationService _notificationService;
        private readonly ITransactionService _transactionService;

        public GroupFundsService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            IRedisService redisService,
            IMailService mailService,
            INotificationService notificationService,
            ITransactionService transactionService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _redisService = redisService;
            _mailService = mailService;
            _notificationService = notificationService;
            _transactionService = transactionService;
        }

        public async Task<BaseResultModel> CreateGroupFundsAsync(CreateGroupModel model)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(model.AccountBankId);
            if (bankAccount == null)
            {
                throw new NotExistException("", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // Map the model to a new GroupFund entity and set its Id to the one generated for groupEntity
            var groupFund = _mapper.Map<GroupFund>(model);
            groupFund.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            groupFund.Status = GroupStatus.ACTIVE;
            groupFund.Visibility = VisibilityEnum.PRIVATE;
            groupFund.CreatedBy = user.Email;
            groupFund.AccountBankId = model.AccountBankId;

            groupFund.GroupMembers = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = user.Id,
                    ContributionPercentage = 100,
                    Role = RoleGroup.LEADER,
                    Status = GroupMemberStatus.ACTIVE,
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = user.Email,
                }
            };

            groupFund.GroupFundLogs = new List<GroupFundLog>
            {
                new GroupFundLog
                {
                    ChangedBy = user.FullName,
                    ChangeDescription = "đã tạo nhóm",
                    Action = GroupAction.CREATED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = user.Email
                }
            };
            // Add the groupFund to the repository and save changes again
            await _unitOfWork.GroupFundRepository.AddAsync(groupFund);
            _unitOfWork.Save();

            // add image

            Image newImage = null;

            if (model.Image != null)
            {
                newImage = new Image
                {
                    EntityId = groupFund.Id,
                    EntityName = EntityName.GROUP.ToString(),
                    ImageUrl = model.Image,
                    CreatedBy = user.Email
                };

                await _unitOfWork.ImageRepository.AddAsync(newImage);
                _unitOfWork.Save();
            }

            var result = _mapper.Map<GroupFundModel>(groupFund);
            result.GroupMembers = new List<GroupMemberModel>();
            result.ImageUrl = newImage?.ImageUrl != null ? newImage.ImageUrl : null;

            // Return a success result with the created groupFund
            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Data = result,
                Message = MessageConstants.GROUP_CREATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> GetAllGroupFunds(PaginationParameter paginationParameters, GroupFilter filter)
        {
            // check current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            if (currentUser.Role == RolesEnum.ADMIN)
            {
                // Get all groupFunds
                var groupFunds = await _unitOfWork.GroupFundRepository.GetGroupFundsFilterAsync(paginationParameters, filter);
                var groupFundModels = _mapper.Map<List<GroupFundModel>>(groupFunds);

                // Get and map images for each group fund
                var images = await _unitOfWork.ImageRepository.GetImagesByEntityNameAsync(EntityName.GROUP.ToString());
                foreach (var groupFund in groupFundModels)
                {
                    var image = images.FirstOrDefault(i => i.EntityId == groupFund.Id);
                    groupFund.ImageUrl = image?.ImageUrl != null ? image.ImageUrl : null;
                }

                var groupPagingResult = PaginationHelper.GetPaginationResult(groupFunds, groupFundModels);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = groupPagingResult
                };
            }
            else
            {
                // setup filter
                filter.UserId = currentUser.Id;

                // Get all group's user
                var groupFunds = await _unitOfWork.GroupFundRepository.GetGroupFundsFilterAsync(
                    paginationParameters,
                    filter,
                    include: q => q
                        .Include(x => x.GroupMembers)
                );

                // remove group member in list
                foreach (var group in groupFunds)
                {
                    group.GroupMembers = null;
                }

                var groupFundModels = _mapper.Map<List<GroupFundModel>>(groupFunds);

                // Get and map images for each group fund
                var images = await _unitOfWork.ImageRepository.GetImagesByEntityNameAsync(EntityName.GROUP.ToString());
                foreach (var groupFund in groupFundModels)
                {
                    var image = images.FirstOrDefault(i => i.EntityId == groupFund.Id);
                    groupFund.ImageUrl = image?.ImageUrl != null ? image.ImageUrl : null;
                }

                var groupPagingResult = PaginationHelper.GetPaginationResult(groupFunds, groupFundModels);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = groupPagingResult
                };
            }

        }

        public async Task<BaseResultModel> CloseGroupFundAsync(Guid groupId)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: q => q
                    .Include(x => x.GroupFundLogs)
                    .Include(x => x.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_CLOSE_FORBIDDEN
                };
            }

            // Check if the group has any transactions
            if (groupFund.Transactions.Any())
            {
                // check group balance = 0
                if (groupFund.CurrentBalance > 0)
                {
                    throw new DefaultException(MessageConstants.GROUP_BALANCE_MUST_EQUAL_ZERO_MESSAGE, 
                        MessageConstants.GROUP_BALANCE_MUST_EQUAL_ZERO);
                }

                // Soft delete: mark the group as inactive
                groupFund.Status = GroupStatus.DISBANDED;
                _unitOfWork.GroupFundRepository.SoftDeleteAsync(groupFund);

                // Add a log entry for the disband group action
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = currentUser.FullName,
                    ChangeDescription = $"đã chuyển nhóm vào chế độ lưu trữ",
                    Action = GroupAction.DISBANDED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                });
            }
            else
            {
                // Hard delete: remove the group from the database
                // BR: xóa cứng nhóm nếu chưa có transaction nào

                _unitOfWork.GroupFundLogRepository.PermanentDeletedListAsync(groupFund.GroupFundLogs.ToList());

                // get group member
                var groupMembers = groupFund.GroupMembers;

                // delete member
                _unitOfWork.GroupMemberRepository.PermanentDeletedListAsync(groupMembers.ToList());

                // delete group
                _unitOfWork.GroupFundRepository.PermanentDeletedAsync(groupFund);
            }



            // Save the changes to the repository
            await _unitOfWork.SaveAsync();

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_CLOSE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> RemoveMemberByLeaderAsync(Guid groupId, Guid memberId)
        {
            // BR: leader chỉ có thể xóa thành viên nếu thành viên chưa đóng góp vào nhóm (chưa có transaction)
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: query => query.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var leader = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (leader == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.GROUP_REMOVE_MEMBER_FORBIDDEN
                };
            }

            if (leader.UserId == memberId)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.GROUP_CAN_NOT_REMOVE_LEADER,
                    Message = "Cannot remove yourself from the group"
                };
            }


            // Find the member to be removed
            var memberToRemove = groupFund.GroupMembers
                .FirstOrDefault(member => member.UserId == memberId && member.Status == GroupMemberStatus.ACTIVE);
            if (memberToRemove == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            // kiểm tra % contribution đối với group có goal
            var groupGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: g => g.GroupId == groupId && g.Status == FinancialGoalStatus.ACTIVE
            );

            if (groupGoal.Any())
            {
                if (memberToRemove.ContributionPercentage > 0)
                {
                    throw new DefaultException(MessageConstants.GROUP_MEMBER_HAVE_CONTRIBUTION_MESSAGE,
                        MessageConstants.GROUP_MEMBER_HAVE_CONTRIBUTION);
                }
            }

            // get remove member info
            var removeMember = await _unitOfWork.UsersRepository.GetByIdAsync(memberId);
            if (removeMember == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            if (memberToRemove.Status == GroupMemberStatus.PENDING)
            {
                _unitOfWork.GroupMemberRepository.PermanentDeletedAsync(memberToRemove);
                await _unitOfWork.SaveAsync();

                // Return a success result
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.GROUP_REMOVE_MEMBER_SUCCESS_MESSAGE
                };
            }
            else
            {
                // kiểm tra xem thành viên đã có giao dịch nào chưa
                var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                    filter: t => t.GroupId == groupId && t.UserId == memberToRemove.UserId && t.Status == TransactionStatus.APPROVED
                );
                if (transactions.Any())
                {
                    throw new DefaultException(MessageConstants.GROUP_MEMBER_HAVE_TRANSACTION_MESSAGE, MessageConstants.GROUP_MEMBER_HAVE_TRANSACTION);
                }

                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = currentUser.FullName,
                    ChangeDescription = $"đã xóa {removeMember.FullName} khỏi nhóm",
                    Action = GroupAction.UPDATED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                });

                memberToRemove.Status = GroupMemberStatus.INACTIVE;
                memberToRemove.UpdatedBy = currentUser.Email;
                _unitOfWork.GroupMemberRepository.SoftDeleteAsync(memberToRemove);

                await _unitOfWork.SaveAsync();

                // send notification to member
                var newNotification = new Notification
                {
                    UserId = memberId,
                    Title = $"Rời khỏi nhóm {groupFund.Name}",
                    Message = $"Bạn đã bị {currentUser.FullName} xóa khỏi nhóm '{groupFund.Name}'. Ấn vào thông báo để xem chi tiết.",
                    EntityId = groupFund.Id,
                    Type = NotificationType.GROUP,
                };

                await _notificationService.AddNotificationByUserId(memberId, newNotification);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.GROUP_REMOVE_MEMBER_SUCCESS_MESSAGE
                };
            }
        }

        /// <summary>
        /// cập nhật vai trò trong nhóm (chỉ leader mới có quyền cập nhật)
        /// 

        public async Task<BaseResultModel> SetMemberRoleAsync(SetRoleGroupModel setRoleGroupModel)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(setRoleGroupModel.GroupId, include: query => query.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_SET_ROLE_FORBIDDEN
                };
            }

            // Find the member whose role is to be changed
            var memberToUpdate = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == setRoleGroupModel.MemberId);
            if (memberToUpdate == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_MEMBER_NOT_FOUND
                };
            }

            if (memberToUpdate.UserId == currentUser.Id)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_SET_ROLE_FORBIDDEN
                };
            }

            if (memberToUpdate.Role == setRoleGroupModel.RoleGroup)
            {
                throw new DefaultException($"Member already '{setRoleGroupModel.RoleGroup.ToString()}' on group",
                    MessageConstants.GROUP_MEMBER_ALREADY_ROLE);
            }

            memberToUpdate.Role = setRoleGroupModel.RoleGroup;

            var memberUpdateInfo = await _unitOfWork.UsersRepository.GetByIdAsync(memberToUpdate.UserId);

            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã thay đổi vai trò của " +
                    $"{memberUpdateInfo.FullName} thành {setRoleGroupModel.RoleGroup.ToString()}",
                Action = GroupAction.UPDATED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            groupFund.UpdatedBy = currentUser.Email;

            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // send notification to member
            var newNotification = new Notification
            {
                UserId = memberToUpdate.UserId,
                Title = $"Thay đổi vai trò trong nhóm {groupFund.Name}",
                Message = $"Vai trò của bạn trong nhóm '{groupFund.Name}' đã được thay đổi thành '{setRoleGroupModel.RoleGroup.ToString()}'. Ấn vào thông báo để xem chi tiết.",
                EntityId = groupFund.Id,
                Type = NotificationType.GROUP,
            };

            await _notificationService.AddNotificationByUserId(memberToUpdate.UserId, newNotification);

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.MEMBER_ROLE_UPDATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> GenerateFinancialHealthReportAsync(Guid groupId)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdAsync(groupId);
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Calculate financial ratios
            var totalIncome = groupFund.Transactions.Where(t => t.Type == TransactionType.INCOME).Sum(t => (int?)t.Amount) ?? 0;
            var totalDebt = groupFund.Transactions.Where(t => t.Type == TransactionType.EXPENSE).Sum(t => (int?)t.Amount) ?? 0;
            var totalSavings = totalIncome - totalDebt;

            var savingRatio = (totalSavings / totalIncome) * 100;
            var debtToIncomeRatio = (totalDebt / totalIncome) * 100;
            var netWorth = totalSavings;

            // Generate suggestions based on financial ratios 
            // Import AI generated suggestions here
            var suggestions = new List<string>();
            if (savingRatio < 10)
            {
                suggestions.Add("Tiết kiệm hiện tại là dưới 10%. Hãy đặt mục tiêu tiết kiệm ít nhất 20% thu nhập.");
            }
            if (debtToIncomeRatio > 50)
            {
                suggestions.Add("Nợ chiếm hơn 50% thu nhập. Ưu tiên thanh toán nợ tín dụng trước để giảm gánh nặng lãi suất.");
            }
            //
            // Create the financial health report
            var report = new FinancialHealthReport
            {
                SavingRatio = savingRatio,
                DebtToIncomeRatio = debtToIncomeRatio,
                NetWorth = netWorth,
                Suggestions = suggestions
            };

            // Return the report
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = report,
                Message = MessageConstants.REPORT_GENERATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> InviteMemberEmailAsync(InviteMemberModel inviteMemberModel)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(inviteMemberModel.GroupId, include: query => query.Include(c => c.GroupMembers));

            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_INVITE_FORBIDDEN_MESSAGE
                };
            }

            var users = await _unitOfWork.UsersRepository.GetAllAsync();
            var emailSet = new HashSet<string>(inviteMemberModel.Emails);

            var invitedUsers = users.Where(u => emailSet.Contains(u.Email)).ToList();

            if (invitedUsers.Count != emailSet.Count)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // check list member
            foreach (var invitedUser in invitedUsers) 
            {
                // check member is exist in group
                var memberExist = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == invitedUser.Id);
                if (memberExist != null && memberExist.Status == GroupMemberStatus.ACTIVE)
                {
                    throw new DefaultException("", MessageConstants.GROUP_MEMBER_EXIST);
                }
                else if (memberExist == null)
                {
                    // Add the member to the group with a pending status
                    var pendingMember = new GroupMember
                    {
                        UserId = invitedUser.Id,
                        ContributionPercentage = 0,
                        Role = RoleGroup.MEMBER,
                        Status = GroupMemberStatus.PENDING,
                        CreatedDate = CommonUtils.GetCurrentTime(),
                        CreatedBy = currentUser.Email,
                    };
                    groupFund.GroupMembers.Add(pendingMember);
                }

                // Generate a raw invitation token
                var rawToken = Guid.NewGuid().ToString();

                // hash token use hmac sha256
                var hashedToken = StringUtils.HashToken(rawToken);

                var invitationLink = $"https://easymoney.anttravel.online/moneyez-web/accept-invitation?token={HttpUtility.UrlEncode(hashedToken)}";
                //var invitationLink = $"http://localhost:3000/moneyez-web/accept-invitation?token={HttpUtility.UrlEncode(invitationToken)}";

                // Save the invitation token to Redis
                var redisKey = hashedToken;
                var groupInviteRedisModel = new GroupInviteRedisModel
                {
                    GroupId = inviteMemberModel.GroupId,
                    UserId = invitedUser.Id
                };
                await _redisService.SetAsync(redisKey, groupInviteRedisModel, TimeSpan.FromDays(1));

                string defaultDescription = $"Bạn đã được '{currentUser.FullName}' mời vào nhóm '{groupFund.Name}'. Ấn vào link để tham gia: {invitationLink}";

                string emailBody = SendInviteGroupMember
                    .EmailSendInviteGroupMember(currentUser.FullName, invitedUser.FullName, groupFund.Name, 
                        string.IsNullOrEmpty(inviteMemberModel.Description) ? defaultDescription : inviteMemberModel.Description, invitationLink);

                // send mail
                MailRequest newEmail = new MailRequest()
                {
                    ToEmail = invitedUser.Email,
                    Subject = $"[MoneyEz] Lời mời tham gia nhóm {groupFund.Name}",
                    Body = emailBody,
                };

                // send mail
                await _mailService.SendEmailAsync_v2(newEmail);

                // Add a log entry for the invite member action
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = currentUser.FullName,
                    ChangeDescription = $"đã mời {invitedUser.FullName} vào nhóm qua email",
                    Action = GroupAction.INVITED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                });

                // Save the changes to the repository
                groupFund.UpdatedBy = currentUser.Email;
                _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);

                await _unitOfWork.SaveAsync();

                // send notification to member
                var newNotification = new Notification
                {
                    UserId = invitedUser.Id,
                    Title = $"Lời mời tham gia nhóm [{groupFund.Name}]",
                    Message = string.IsNullOrEmpty(inviteMemberModel.Description) ?
                        $"Bạn đã được '{currentUser.FullName}' mời vào nhóm '{groupFund.Name}'. Tham gia ngay!"
                        : $"{inviteMemberModel.Description}",
                    EntityId = groupFund.Id,
                    Href = invitationLink,
                    Type = NotificationType.GROUP_INVITE,
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                };

                await _notificationService.AddNotificationByUserId(invitedUser.Id, newNotification);
            }

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_INVITE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> AcceptInvitationEmailAsync(string token)
        {
            // Retrieve the invitation token from Redis
            var groupInviteRedisModel = await _redisService.GetAsync<GroupInviteRedisModel>(token);
            if (groupInviteRedisModel == null || groupInviteRedisModel.UserId == Guid.Empty)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = MessageConstants.INVALID_INVITATION_TOKEN_MESSAGE
                };
            }

            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupInviteRedisModel.GroupId, include: query => query.Include(c => c.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Retrieve the user by email
            var userInvite = await _unitOfWork.UsersRepository.GetByIdAsync(groupInviteRedisModel.UserId);
            if (userInvite == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.ACCOUNT_NOT_EXIST
                };
            }

            // check member is exist
            var memberExist = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == userInvite.Id);
            if (memberExist != null && memberExist.Status == GroupMemberStatus.ACTIVE)
            {
                throw new DefaultException("", MessageConstants.GROUP_MEMBER_EXIST);
            }
            else if (memberExist != null && memberExist.Status != GroupMemberStatus.ACTIVE)
            {
                memberExist.IsDeleted = false;
                memberExist.Status = GroupMemberStatus.ACTIVE;
                memberExist.UpdatedDate = CommonUtils.GetCurrentTime();

                // add log
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = userInvite.FullName,
                    ChangeDescription = $"đã tham gia nhóm qua email",
                    Action = GroupAction.JOINED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = userInvite.Email
                });

                // Save the changes to the repository
                groupFund.UpdatedBy = userInvite.Email;
                _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
                await _unitOfWork.SaveAsync();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE
                };
            }
            else
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_MEMBER_NOT_FOUND
                };
            }
        }

        public async Task<BaseResultModel> GetGroupFundById(Guid groupId)
        {
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: q => q.Include(c => c.GroupMembers).ThenInclude(gm => gm.User));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(groupFund.Id, EntityName.GROUP.ToString());
            var groupFundModel = _mapper.Map<GroupFundModel>(groupFund);
            groupFundModel.ImageUrl = images.FirstOrDefault()?.ImageUrl;

            // get goal
            var groupGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: g => g.GroupId == groupId && g.Status == FinancialGoalStatus.ACTIVE
            );

            if (groupGoal.Any())
            {
                groupFundModel.IsGoalActive = true;
            }

            var allGroupTransactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.GroupId == groupId && t.Type == TransactionType.INCOME && t.Status == TransactionStatus.APPROVED
            );

            if (groupFundModel.GroupMembers != null && groupFundModel.GroupMembers.Any())
            {
                foreach (var member in groupFundModel.GroupMembers)
                {
                    var memberTransactions = allGroupTransactions.Where(t => t.UserId == member.UserId).ToList();

                    decimal totalContribution = memberTransactions.Sum(t => t.Amount);

                    member.TotalContribution = totalContribution;

                    member.TransactionCount = memberTransactions.Count;
                }
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = groupFundModel
            };
        }

        public async Task<BaseResultModel> InviteMemberQRCodeAsync(InviteMemberModel inviteMemberModel)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(inviteMemberModel.GroupId, include: query => query.Include(c => c.GroupMembers));

            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_INVITE_FORBIDDEN_MESSAGE
                };
            }

            // generate inivitation code
            var invitationCode = "";
            while (true)
            {
                invitationCode = StringUtils.GenerateInviteCode();
                var existingCode = await _redisService.GetAsync<GroupInviteRedisModel>(invitationCode);
                if (existingCode == null)
                {
                    break;
                }
            }

            // Save the invitation token to Redis
            var redisKey = invitationCode;
            var groupInviteRedisModel = new GroupInviteRedisModel
            {
                InviteToken = invitationCode,
                GroupId = inviteMemberModel.GroupId
            };
            await _redisService.SetAsync(redisKey, groupInviteRedisModel, TimeSpan.FromMinutes(10));

            // Add a log entry for the invite member action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã tạo liên kết mời vào nhóm",
                Action = GroupAction.INVITED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            // Save the changes to the repository
            groupFund.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new QRCodeInviteModel
                {
                    QRCode = invitationCode,
                    ExpiredTime = CommonUtils.GetCurrentTime().AddMinutes(10)
                },
                Message = "Đã tạo mã QRCode mời vào nhóm. Mã có hiệu lực trong 10 phút"
            };
        }

        public async Task<BaseResultModel> AcceptInvitationQRCodeAsync(string token)
        {
            // Retrieve the invitation token from Redis
            var groupInviteRedisModel = await _redisService.GetAsync<GroupInviteRedisModel>(token);
            if (groupInviteRedisModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = MessageConstants.INVALID_INVITATION_TOKEN_MESSAGE
                };
            }

            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupInviteRedisModel.GroupId, include: query => query.Include(c => c.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // get current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);

            // check member is exist
            var memberExist = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == currentUser.Id);
            if (memberExist != null && memberExist.Status == GroupMemberStatus.ACTIVE)
            {
                throw new DefaultException("", MessageConstants.GROUP_MEMBER_EXIST);
            }
            else if (memberExist != null && memberExist.Status != GroupMemberStatus.ACTIVE)
            {
                memberExist.IsDeleted = false;
                memberExist.Status = GroupMemberStatus.ACTIVE;
                memberExist.UpdatedDate = CommonUtils.GetCurrentTime();

                // Add a log entry for the invite member action
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = currentUser.FullName,
                    ChangeDescription = $"đã tham gia nhóm qua liên kết mời",
                    Action = GroupAction.JOINED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                });

                // Save the changes to the repository
                _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
                await _unitOfWork.SaveAsync();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE
                };
            }

            // Add the member to the group
            var newMember = new GroupMember
            {
                UserId = currentUser.Id,
                ContributionPercentage = 0,
                Role = RoleGroup.MEMBER,
                Status = GroupMemberStatus.ACTIVE,
                CreatedDate = CommonUtils.GetCurrentTime()
            };

            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã tham gia nhóm qua liên kết mời",
                Action = GroupAction.JOINED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            groupFund.GroupMembers.Add(newMember);

            // Save the changes to the repository
            groupFund.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE,
                Data = new
                {
                    GroupId = groupFund.Id
                }
            };
        }

        public async Task<BaseResultModel> LeaveGroupAsync(Guid groupId)
        {
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: query => query.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.GROUP_CAN_NOT_REMOVE_LEADER,
                    Message = "Cannot remove yourself from the group"
                };
            }

            // Find the member to be removed
            var memberToRemove = groupFund.GroupMembers
                .FirstOrDefault(member => member.UserId == currentUser.Id && member.Status == GroupMemberStatus.ACTIVE);
            if (memberToRemove == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            // kiểm tra % contribution đối với group có goal
            var groupGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: g => g.GroupId == groupId && g.Status == FinancialGoalStatus.ACTIVE
            );

            if (groupGoal.Any())
            {
                if (memberToRemove.ContributionPercentage > 0)
                {
                    throw new DefaultException(MessageConstants.GROUP_MEMBER_HAVE_CONTRIBUTION_MESSAGE,
                        MessageConstants.GROUP_MEMBER_HAVE_CONTRIBUTION);
                }
            }

            // add log
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"{currentUser.FullName} đã rời khỏi nhóm",
                Action = GroupAction.LEFT.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            memberToRemove.Status = GroupMemberStatus.INACTIVE;
            memberToRemove.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupMemberRepository.SoftDeleteAsync(memberToRemove);

            // Save the changes to the repository
            await _unitOfWork.SaveAsync();

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_LEAVE_SUCCESS_MESSAGE
            };
        }

        /// <summary>
        /// cập nhật tỉ lệ đóng góp của các thành viên trong nhóm (chỉ leader mới có quyền cập nhật)
        /// 
        public async Task<BaseResultModel> SetGroupContribution(SetGroupContributionModel setGroupContributionModel)
        {
            // Retrieve the group fund by its Id including members
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(setGroupContributionModel.GroupId, include: query => query.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Check if the current user exists and is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                throw new DefaultException("Only group leader can set contribution percentages",
                    MessageConstants.GROUP_SET_CONTRIBUTION_FORBIDDEN);
            }

            // Validate total contribution equals 100%
            var totalContribution = setGroupContributionModel.MemberContributions.Sum(x => x.Contribution);
            if (totalContribution != 100)
            {
                throw new DefaultException("Total contribution percentage must equal 100%",
                    MessageConstants.GROUP_INVALID_TOTAL_CONTRIBUTION);
            }

            // Update contribution percentages for each member
            foreach (var memberContribution in setGroupContributionModel.MemberContributions)
            {
                var groupMember = groupFund.GroupMembers.FirstOrDefault(m =>
                    m.UserId == memberContribution.MemberId &&
                    m.Status == GroupMemberStatus.ACTIVE);

                if (groupMember == null)
                {
                    throw new NotExistException($"Member with ID {memberContribution.MemberId} not found in group",
                        MessageConstants.GROUP_MEMBER_CONTRIBUTION_NOT_FOUND);
                }

                groupMember.ContributionPercentage = memberContribution.Contribution;
                groupMember.UpdatedDate = CommonUtils.GetCurrentTime();
            }

            // Add log entry for contribution update
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã cập nhật tỷ lệ đóng góp cho các thành viên",
                Action = GroupAction.UPDATED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            groupFund.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // send notification to members

            var newNotification = new Notification
            {
                Title = $"Cập nhật tỷ lệ đóng góp nhóm {groupFund.Name}",
                Message = $"Tỷ lệ đóng góp của bạn trong nhóm '{groupFund.Name}' đã được cập nhật. Ấn vào thông báo để xem chi tiết.",
                EntityId = groupFund.Id,
                Type = NotificationType.GROUP,
            };

            var listUserId = groupFund.GroupMembers.Select(x => x.UserId).ToList();

            await _notificationService.AddNotificationByListUser(listUserId, newNotification);


            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_SET_CONTRIBUTION_SUCCESS_MESSAGE
            };
        }

        /// <summary>
        /// tạo yêu cầu góp quỹ vào nhóm
        /// 
        public async Task<BaseResultModel> CreateFundraisingRequest(CreateFundraisingModel createFundraisingModel)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Get group with bank account info
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                createFundraisingModel.GroupId,
                include: q => q.Include(g => g.GroupMembers));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            var groupBankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(groupFund.AccountBankId.Value);
            if (groupBankAccount == null)
            {
                throw new NotExistException("", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // Verify user is a member of the group
            var isMember = groupFund.GroupMembers.Any(member =>
                member.UserId == currentUser.Id &&
                member.Status == GroupMemberStatus.ACTIVE);

            if (!isMember)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            // Create a new fundraising request
            var newFundraisingRequest = new CreateGroupTransactionModel
            {
                GroupId = createFundraisingModel.GroupId,
                Description = createFundraisingModel.Description,
                Amount = createFundraisingModel.Amount,
                Type = TransactionType.INCOME
            };

            return await _transactionService.CreateGroupTransactionAsync(newFundraisingRequest, currentUser.Email);
        }

        public async Task<GroupMember> GetGroupLeader(Guid groupId)
        {
            // Get group with members
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: q => q
                    .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Find leader member
            var leader = groupFund.GroupMembers.FirstOrDefault(member =>
                member.Role == RoleGroup.LEADER &&
                member.Status == GroupMemberStatus.ACTIVE &&
                !member.IsDeleted);

            if (leader == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_LEADER_NOT_FOUND);
            }

            return leader;
        }
        
        public async Task<BaseResultModel> GetGroupFundLogs(Guid groupId, PaginationParameter paginationParameters, GroupLogFilter filter)
        {
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Kiểm tra xem user có thuộc nhóm quỹ không
            var groupMembers = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == groupId && gm.UserId == currentUser.Id,
                include: gm => gm.Include(gm => gm.User)
            );

            if (!groupMembers.Any())
            {
                throw new DefaultException("You can not access this group.", MessageConstants.GROUP_ACCESS_DENIED);
            }

            var logsPagination = await _unitOfWork.GroupFundLogRepository.GetGroupFundLogsFilter(
                   paginationParameters,
                   filter,
                   condition: log => log.GroupId == groupId
             );

            var groupFundLogModels = _mapper.Map<List<GroupFundLogModel>>(logsPagination);

            var allUser = await _unitOfWork.UsersRepository.GetAllAsync();
            foreach (var log in groupFundLogModels)
            {
                var user = allUser.FirstOrDefault(u => u.Email == log.CreatedBy);
                if (user != null)
                {
                    log.ImageUrl = user.AvatarUrl;
                }
            }

            var result = PaginationHelper.GetPaginationResult(logsPagination, groupFundLogModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Retrieved group fund logs successfully",
                Data = result
            };
        }
        
        public async Task<List<GroupMember>> GetGroupMembers(Guid groupId)
        {
            // Get group with members
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: q => q
                    .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Get all active members
            var members = groupFund.GroupMembers
                .Where(member =>
                    member.Status == GroupMemberStatus.ACTIVE &&
                    !member.IsDeleted)
                .ToList();

            return members;
        }

        /// <summary>
        /// tạo yêu cầu rút tiền từ quỹ nhóm
        /// 
        public async Task<BaseResultModel> CreateFundWithdrawalRequest(CreateFundWithdrawalModel createFundWithdrawalModel)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Get group with bank account info
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                createFundWithdrawalModel.GroupId,
                include: q => q.Include(g => g.GroupMembers));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // validate amount
            if (createFundWithdrawalModel.Amount > groupFund.CurrentBalance)
            {
                throw new DefaultException("Amount must be less than or equal to the balance",
                    MessageConstants.GROUP_WITHDRAWAL_AMOUNT_INVALID);
            }

            var groupBankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(groupFund.AccountBankId.Value);
            if (groupBankAccount == null)
            {
                throw new NotExistException("", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // Verify user is a member of the group
            var isMember = groupFund.GroupMembers.Any(member =>
                member.UserId == currentUser.Id &&
                member.Status == GroupMemberStatus.ACTIVE);

            if (!isMember)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            // Create a new fundraising request
            var newFundraisingRequest = new CreateGroupTransactionModel
            {
                GroupId = createFundWithdrawalModel.GroupId,
                Description = createFundWithdrawalModel.Description,
                Amount = createFundWithdrawalModel.Amount,
                Type = TransactionType.EXPENSE,
                Images = createFundWithdrawalModel.Images
            };

            return await _transactionService.CreateGroupTransactionAsync(newFundraisingRequest, currentUser.Email);
        }

        /// <summary>
        /// tạo lời nhắc góp quỹ cho các thành viên trong nhóm (chỉ leader mới có quyền tạo)
        /// 
        public async Task<BaseResultModel> RemindFundraisingAsync(RemindFundraisingModel remindFundraisingModel)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Get group with members and financial goals
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                remindFundraisingModel.GroupId,
                include: q => q.Include(g => g.GroupMembers).Include(g => g.FinancialGoals));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Validate leader permission
            var isLeader = groupFund.GroupMembers.Any(member =>
                member.UserId == currentUser.Id &&
                member.Role == RoleGroup.LEADER &&
                member.Status == GroupMemberStatus.ACTIVE);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_REMIND_FORBIDDEN
                };
            }

            // Get active financial goal if exists
            var activeGoal = groupFund.FinancialGoals
                .FirstOrDefault(g => g.Status == FinancialGoalStatus.ACTIVE);

            // Xác định tổng số tiền cần đóng góp
            decimal totalRequired = 0;
            bool hasActiveGoal = activeGoal != null;

            if (hasActiveGoal)
            {
                totalRequired = activeGoal.TargetAmount;

                // Kiểm tra tổng % đóng góp chỉ khi có mục tiêu tài chính
                var totalPercentage = groupFund.GroupMembers
                    .Where(m => m.Status == GroupMemberStatus.ACTIVE)
                    .Sum(m => m.ContributionPercentage ?? 0);

                if (totalPercentage != 100)
                {
                    throw new DefaultException("Total contribution percentage must equal 100%",
                        MessageConstants.GROUP_INVALID_TOTAL_CONTRIBUTION);
                }
            }

            // Process each member
            foreach (var memberRemind in remindFundraisingModel.Members)
            {
                var groupMember = groupFund.GroupMembers.FirstOrDefault(m =>
                    m.UserId == memberRemind.MemberId &&
                    m.Status == GroupMemberStatus.ACTIVE);

                if (groupMember == null)
                {
                    throw new NotExistException($"Member with ID {memberRemind.MemberId} not found in group",
                        MessageConstants.GROUP_MEMBER_NOT_FOUND);
                }

                // Get member user info
                var memberUser = await _unitOfWork.UsersRepository.GetByIdAsync(memberRemind.MemberId);
                if (memberUser == null)
                {
                    throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
                }

                // Xác định số tiền cuối cùng dựa vào trường hợp có goal hoặc không
                decimal finalAmount;

                if (hasActiveGoal)
                {
                    // Trường hợp có goal: Tính theo % đóng góp
                    decimal expectedAmount = totalRequired * (groupMember.ContributionPercentage ?? 0) / 100m;
                    // Nếu amount đưa xuống vượt quá số tiền dự kiến, lấy giá trị dự kiến
                    finalAmount = Math.Min(memberRemind.Amount, expectedAmount);
                }
                else
                {
                    // Trường hợp không có goal: Lấy trực tiếp amount nhập vào
                    finalAmount = memberRemind.Amount;
                }

                // Send notification
                var notification = new Notification
                {
                    UserId = memberRemind.MemberId,
                    Title = $"Nhắc nhở góp quỹ [{groupFund.Name}]",
                    Message = $"Bạn cần đóng góp {finalAmount:N0} VNĐ cho quỹ nhóm '{groupFund.Name}",
                    EntityId = groupFund.Id,
                    Type = NotificationType.GROUP,
                };
                await _notificationService.AddNotificationByUserId(memberRemind.MemberId, notification);

                // create transaction pending
                var groupTransaction = new CreateGroupTransactionModel
                {
                    GroupId = groupFund.Id,
                    Description = $"Nhắc nhở góp quỹ cho thành viên {memberUser.FullName}",
                    Amount = finalAmount,
                    Type = TransactionType.INCOME,
                    TransactionDate = CommonUtils.GetCurrentTime()
                };

                var transactionResult = await _transactionService.CreateGroupTransactionAsync(groupTransaction, memberUser.Email);
            }

            // Add group fund log
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã gửi nhắc nhở góp quỹ" + (activeGoal != null ? $" cho mục tiêu '{activeGoal.Name}'" : ""),
                Action = GroupAction.TRANSACTION_CREATED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            // Save changes
            groupFund.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_REMIND_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> UpdateGroupFundsAsync(UpdateGroupModel model)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(model.AccountBankId);
            if (bankAccount == null)
            {
                throw new NotExistException("", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // get group fund
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(model.Id);
            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // check user is leader
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == user.Id 
                && member.Role == RoleGroup.LEADER && member.Status == GroupMemberStatus.ACTIVE);

            if (!isLeader)
            {
                throw new DefaultException(MessageConstants.GROUP_UPDATE_FORBIDDEN_MESSAGE, MessageConstants.GROUP_UPDATE_FORBIDDEN);
            }

            // Map the model to the existing groupFund entity
            _mapper.Map(model, groupFund);
            groupFund.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            groupFund.UpdatedBy = user.Email;

            // group image
            var groupImages = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(groupFund.Id, EntityName.GROUP.ToString());
            var groupImage = groupImages.FirstOrDefault();


            if (groupImage == null)
            {
                Image newImage = null;

                if (model.Image != null)
                {
                    newImage = new Image
                    {
                        EntityId = groupFund.Id,
                        EntityName = EntityName.GROUP.ToString(),
                        ImageUrl = model.Image,
                        CreatedBy = user.Email
                    };

                    await _unitOfWork.ImageRepository.AddAsync(newImage);
                    await _unitOfWork.SaveAsync();
                }
            }
            else
            {
                if (model.Image != null)
                {
                    // Update the existing image entity
                    groupImage.ImageUrl = model.Image;
                    groupImage.UpdatedBy = user.Email;
                    _unitOfWork.ImageRepository.UpdateAsync(groupImage);
                    await _unitOfWork.SaveAsync();
                }
            }

            groupFund.GroupFundLogs = new List<GroupFundLog>
            {
                new GroupFundLog
                {
                    ChangedBy = user.FullName,
                    ChangeDescription = "đã chỉnh sửa thông tin của nhóm",
                    Action = GroupAction.CREATED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = user.Email
                }
            };
            // Add the groupFund to the repository and save changes again
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // Return a success result with the created groupFund
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_UPDATE_SUCCESS_MESSAGE
            };
        }

        public async Task LogGroupFundChange(Guid groupId, string description, GroupAction action, string userEmail)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException("User not found", MessageConstants.ACCOUNT_NOT_EXIST);
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdAsync(groupId)
                ?? throw new NotExistException("Group not found", MessageConstants.GROUP_NOT_EXIST);
            var log = new GroupFundLog
            {
                GroupId = groupFund.Id,
                ChangedBy = user.FullName,
                ChangeDescription = description,
                Action = action.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = user.Email,
            };

            await _unitOfWork.GroupFundLogRepository.AddAsync(log);
            await _unitOfWork.SaveAsync();
        }

        public async Task<BaseResultModel> GetPendingRequestsAsync(Guid groupId, PaginationParameter paginationParameters)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // get group fund
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(groupId, 
                include: g => g.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // check user is leader
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id
                && member.Role == RoleGroup.LEADER && member.Status == GroupMemberStatus.ACTIVE);

            if (isLeader)
            {
                // get all pending requests
                var pendingRequests = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                    paginationParameters,
                    filter: t => t.GroupId == groupId && t.Status == TransactionStatus.PENDING,
                    include: q => q.Include(t => t.User),
                    orderBy: q => q.OrderByDescending(t => t.CreatedDate)
                );

                var pendingRequestModels = _mapper.Map<List<GroupTransactionModel>>(pendingRequests);
                var result = PaginationHelper.GetPaginationResult(pendingRequests, pendingRequestModels);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = result
                };
            }
            else
            {
                // get users pending requests
                var pendingRequests = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                    paginationParameters,
                    filter: t => t.GroupId == groupId && t.UserId == currentUser.Id && t.Status == TransactionStatus.PENDING,
                    include: q => q.Include(t => t.User),
                    orderBy: q => q.OrderByDescending(t => t.CreatedDate)
                );

                var pendingRequestModels = _mapper.Map<List<GroupTransactionModel>>(pendingRequests);
                // For withdrawal transactions, calculate and add withdrawal limit to notes
                foreach (var request in pendingRequestModels.Where(r => r.Type == TransactionType.EXPENSE.ToString()))
                {
                    // Calculate total deposits by the group member
                    var memberTotalDeposits = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                        filter: t => t.GroupId == groupId &&
                                    t.UserId == request.UserId &&
                                    t.Type == TransactionType.INCOME &&
                                    t.Status == TransactionStatus.APPROVED
                    );

                    decimal totalDepositAmount = memberTotalDeposits.Sum(t => t.Amount);

                    // Calculate current balance
                    var memberBalance = await CalculateMemberBalanceAsync(groupId, request.UserId);

                    // Determine maximum withdrawal limit
                    decimal maxWithdrawalLimit = Math.Min(totalDepositAmount, memberBalance);

                    // Add withdrawal limit info to notes
                    request.Note = maxWithdrawalLimit > 0 ? 
                        $"Hạn mức rút tối đa (khuyến nghị) của thành viên là: {maxWithdrawalLimit:N0} VND" :
                        $"Thành viên chưa góp quỹ hoặc đã rút hết số tiền góp";
                }
                var result = PaginationHelper.GetPaginationResult(pendingRequests, pendingRequestModels);
                
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = result,
                };
            }
        }

        public async Task<BaseResultModel> GetPendingRequestDetailAsync(Guid requestId)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // get pending requests
            var pendingRequest = await _unitOfWork.TransactionsRepository.GetByIdIncludeAsync(
                requestId,
                filter: t => t.Status == TransactionStatus.PENDING,
                include: q => q.Include(t => t.User)
            );

            // get group fund
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(pendingRequest.GroupId.Value,
                include: g => g.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // get group bank account
            var groupBankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(groupFund.AccountBankId.Value);

            if (pendingRequest.Type == TransactionType.INCOME)
            {
                // get info transaction fundraising request
                var response = new
                {
                    Transaction = _mapper.Map<GroupTransactionModel>(pendingRequest),
                    BankAccount = _mapper.Map<BankAccountModel>(groupBankAccount)
                };
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Pending request get successfully",
                    Data = response
                };
            }
            else
            {
                // Calculate total deposits by the group member
                var memberTotalDeposits = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                    filter: t => t.GroupId == groupFund.Id &&
                                t.UserId == pendingRequest.UserId &&
                                t.Type == TransactionType.INCOME &&
                                t.Status == TransactionStatus.APPROVED
                );

                decimal totalDepositAmount = memberTotalDeposits.Sum(t => t.Amount);

                // Calculate current balance
                var memberBalance = await CalculateMemberBalanceAsync(groupFund.Id, pendingRequest.UserId.Value);

                // Determine maximum withdrawal limit
                decimal maxWithdrawalLimit = Math.Min(totalDepositAmount, memberBalance);

                var withdrawalRequest = _mapper.Map<GroupTransactionModel>(pendingRequest);

                // Add withdrawal limit info to notes
                withdrawalRequest.Note = maxWithdrawalLimit > 0 ?
                    $"Hạn mức rút tối đa (khuyến nghị) của thành viên là: {maxWithdrawalLimit:N0} VND" :
                    $"Thành viên chưa góp quỹ hoặc đã rút hết số tiền góp";

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Pending request get successfully",
                    Data = withdrawalRequest
                };
            }
        }

        // Helper method to calculate member's current balance
        private async Task<decimal> CalculateMemberBalanceAsync(Guid groupId, Guid userId)
        {
            // Get all approved transactions for this member
            var memberTransactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.GroupId == groupId &&
                            t.UserId == userId &&
                            t.Status == TransactionStatus.APPROVED
            );

            // Calculate balance (deposits - withdrawals)
            decimal totalDeposits = memberTransactions
                .Where(t => t.Type == TransactionType.INCOME)
                .Sum(t => t.Amount);

            decimal totalWithdrawals = memberTransactions
                .Where(t => t.Type == TransactionType.EXPENSE)
                .Sum(t => t.Amount);

            return totalDeposits - totalWithdrawals;
        }
    }
}
