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

        public GroupFundsService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            IRedisService redisService,
            IMailService mailService,
            INotificationService notificationService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _redisService = redisService;
            _mailService = mailService;
            _notificationService = notificationService;
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
                    GroupMemberLogs = new List<GroupMemberLog>
                    {
                        new GroupMemberLog
                        {
                            ChangeDiscription = $"{user.FullName} đã tham gia nhóm",
                            ChangeType = GroupAction.JOINED,
                            CreatedDate = CommonUtils.GetCurrentTime(),
                            CreatedBy = user.Email
                        }
                    }
                }
            };
            groupFund.GroupFundLogs = new List<GroupFundLog>
            {
                new GroupFundLog
                {
                    ChangeDescription = $"{user.FullName} đã tạo nhóm",
                    Action = GroupAction.CREATED,
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
                    .Include(x => x.GroupMembers).ThenInclude(gm => gm.GroupMemberLogs));
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
                // Soft delete: mark the group as inactive
                groupFund.Status = GroupStatus.DISBANDED;
                _unitOfWork.GroupFundRepository.SoftDeleteAsync(groupFund);

                // Add a log entry for the disband group action
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangeDescription = $"Quỹ đã được đóng bởi {currentUser.FullName}",
                    Action = GroupAction.DELETED,
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

                // delete member logs
                foreach (var member in groupMembers)
                {
                    _unitOfWork.GroupMemberLogRepository.PermanentDeletedListAsync(member.GroupMemberLogs.ToList());
                }

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
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.GROUP_MEMBER_NOT_FOUND
                };
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
                memberToRemove.GroupMemberLogs.Add(new GroupMemberLog
                {
                    ChangeDiscription = $"{removeMember.FullName} đã rời khỏi nhóm",
                    ChangeType = GroupAction.DELETED,
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                });

                memberToRemove.Status = GroupMemberStatus.INACTIVE;
                memberToRemove.UpdatedBy = currentUser.Email;
                _unitOfWork.GroupMemberRepository.SoftDeleteAsync(memberToRemove);

                // Save the changes to the repository
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

                // Return a success result
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.GROUP_REMOVE_MEMBER_SUCCESS_MESSAGE
                };
            }
        }

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

            // Update the member's role
            memberToUpdate.Role = setRoleGroupModel.RoleGroup;

            // get info member update
            var memberUpdateInfo = await _unitOfWork.UsersRepository.GetByIdAsync(memberToUpdate.UserId);

            // Add a log entry for the role change action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = $"{currentUser.FullName} đã thay đổi vai trò của " +
                    $"{memberUpdateInfo.FullName} thành {setRoleGroupModel.RoleGroup.ToString()}",
                Action = GroupAction.UPDATED,
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            groupFund.UpdatedBy = currentUser.Email;

            // Save the changes to the repository
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

            // check invite member
            var inviteUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(inviteMemberModel.Email);
            if (inviteUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // check member is exist in group
            var memberExist = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == inviteUser.Id);
            if (memberExist != null && memberExist.Status == GroupMemberStatus.ACTIVE)
            {
                throw new DefaultException("", MessageConstants.GROUP_MEMBER_EXIST);
            }
            else if (memberExist == null)
            {
                // Add the member to the group with a pending status
                var pendingMember = new GroupMember
                {
                    UserId = inviteUser.Id,
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

            // Hash the token using BCrypt
            var hashedToken = StringUtils.HashToken(rawToken);

            var invitationLink = $"https://easymoney.anttravel.online/api/v1/groups/invite-member/email/accept?token={HttpUtility.UrlEncode(hashedToken)}";
            //var invitationLink = $"https://localhost:7262/api/groups/{inviteMemberModel.GroupId}/accept-invitation?token={HttpUtility.UrlEncode(invitationToken)}";

            // Save the invitation token to Redis
            var redisKey = hashedToken;
            var groupInviteRedisModel = new GroupInviteRedisModel
            {
                InviteToken = hashedToken,
                GroupId = inviteMemberModel.GroupId,
                UserId = inviteUser.Id
            };
            await _redisService.SetAsync(redisKey, groupInviteRedisModel, TimeSpan.FromDays(1));

            // send mail
            MailRequest newEmail = new MailRequest()
            {
                ToEmail = inviteMemberModel.Email,
                Subject = $"[MoneyEz] Lời mời tham gia nhóm {groupFund.Name}",
                Body = $"Bạn đã được '{currentUser.FullName}' mời vào nhóm '{groupFund.Name}'. Ấn vào link để tham gia: {invitationLink}"
            };

            // send mail
            await _mailService.SendEmailAsync_v2(newEmail);

            // Add a log entry for the invite member action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = $"{inviteUser.FullName} đã được mời vào nhóm qua Email",
                Action = GroupAction.INVITED,
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
                UserId = inviteUser.Id,
                Title = $"Lời mời tham gia nhóm {groupFund.Name}",
                Message = $"Bạn đã được '{currentUser.FullName}' mời vào nhóm '{groupFund.Name}'. Ấn vào thông báo để xem chi tiết.",
                EntityId = groupFund.Id,
                Href = invitationLink,
                Type = NotificationType.GROUP_INVITE,
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            };

            await _notificationService.AddNotificationByUserId(inviteUser.Id, newNotification);

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
                memberExist.GroupMemberLogs = new List<GroupMemberLog>
                {
                    new GroupMemberLog
                    {
                        ChangeDiscription = $"{userInvite.FullName} đã tham gia nhóm",
                        ChangeType = GroupAction.JOINED,
                        CreatedDate = CommonUtils.GetCurrentTime(),
                        CreatedBy = userInvite.Email
                    }
                };

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

            // Generate a raw invitation token
            var rawToken = Guid.NewGuid().ToString();

            // Hash the token using BCrypt
            var hashedToken = StringUtils.HashToken(rawToken);

            // Save the invitation token to Redis
            var redisKey = hashedToken;
            var groupInviteRedisModel = new GroupInviteRedisModel
            {
                InviteToken = hashedToken,
                GroupId = inviteMemberModel.GroupId
            };
            await _redisService.SetAsync(redisKey, groupInviteRedisModel, TimeSpan.FromMinutes(10));

            // Add a log entry for the invite member action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = $"{currentUser.FullName} đã tạo liên kết mời vào nhóm",
                Action = GroupAction.INVITED,
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
                    QRCode = hashedToken,
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
                memberExist.GroupMemberLogs = new List<GroupMemberLog>
                {
                    new GroupMemberLog
                    {
                        ChangeDiscription = $"{currentUser.FullName} đã tham gia nhóm",
                        ChangeType = GroupAction.JOINED,
                        CreatedDate = CommonUtils.GetCurrentTime(),
                        CreatedBy = currentUser.Email
                    }
                };

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
                CreatedDate = CommonUtils.GetCurrentTime(),
                GroupMemberLogs = new List<GroupMemberLog>
                {
                    new GroupMemberLog
                    {
                        ChangeDiscription = $"{currentUser.FullName} đã tham gia nhóm",
                        ChangeType = GroupAction.JOINED,
                        CreatedDate = CommonUtils.GetCurrentTime(),
                        CreatedBy = currentUser.Email
                    }
                }
            };

            groupFund.GroupMembers.Add(newMember);

            // Save the changes to the repository
            groupFund.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE
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
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.GROUP_MEMBER_NOT_FOUND
                };
            }

            memberToRemove.GroupMemberLogs.Add(new GroupMemberLog
            {
                ChangeDiscription = $"{currentUser.FullName} đã rời khỏi nhóm",
                ChangeType = GroupAction.DELETED,
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
                ChangeDescription = $"{currentUser.FullName} đã cập nhật tỷ lệ đóng góp cho các thành viên",
                Action = GroupAction.UPDATED,
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            // Save changes
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

            // Generate random 10-digit code
            var requestCode = StringUtils.GenerateRandomUppercaseString(8);

            // Format final request code with bank short name
            var finalRequestCode = $"{requestCode}_{groupBankAccount.BankShortName}";

            // Create new transaction
            var transaction = new Transaction
            {
                GroupId = groupFund.Id,
                UserId = currentUser.Id,
                Amount = createFundraisingModel.Amount,
                Description = createFundraisingModel.Description,
                Type = TransactionType.INCOME,
                Status = TransactionStatus.PENDING,
                RequestCode = finalRequestCode,
                TransactionDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email,
                ApprovalRequired = true
            };

            await _unitOfWork.TransactionsRepository.AddAsync(transaction);
            await _unitOfWork.SaveAsync();

            // Create response with bank account info
            var response = new FundraisingTransactionResponse
            {
                RequestCode = finalRequestCode,
                Amount = transaction.Amount,
                Status = transaction.Status.ToString(),
                BankAccount = _mapper.Map<BankAccountModel>(groupBankAccount)
            };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Fundraising request created successfully",
                Data = response
            };
        }

        public async Task<BaseResultModel> ResponsePendingTransaction(UpdateGroupTransactionModel updateGroupTransactionModel)
        {
            // Get current user and verify existence
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Get group with members to verify leadership
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                updateGroupTransactionModel.GroupId,
                include: q => q.Include(g => g.GroupMembers)
            );

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Verify user is leader of the group
            var isLeader = groupFund.GroupMembers.Any(member =>
                member.UserId == currentUser.Id &&
                member.Role == RoleGroup.LEADER || member.Role == RoleGroup.MOD &&
                member.Status == GroupMemberStatus.ACTIVE);

            if (!isLeader)
            {
                throw new DefaultException("", MessageConstants.TRANSACTION_UPDATE_DENIED);
            }

            // Get and verify transaction
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(updateGroupTransactionModel.TransactionId);
            if (transaction == null)
            {
                throw new NotExistException("", MessageConstants.TRANSACTION_NOT_FOUND);
            }

            // Verify transaction belongs to group
            if (transaction.GroupId != updateGroupTransactionModel.GroupId)
            {
                throw new DefaultException("", MessageConstants.TRANSACTION_NOT_IN_GROUP);
            }

            // Verify transaction is in PENDING status
            if (transaction.Status != TransactionStatus.PENDING)
            {
                throw new DefaultException("", MessageConstants.TRANSACTION_MUST_BE_PENDING);
            }

            // verify transaction is required approval
            if (transaction.ApprovalRequired == false)
            {
                throw new DefaultException("Transaction must be require approval", MessageConstants.TRANSACTION_APPROVE_DENIED);
            }

            // Update transaction status
            transaction.Status = updateGroupTransactionModel.Status;
            transaction.UpdatedBy = currentUser.Email;
            transaction.UpdatedDate = CommonUtils.GetCurrentTime();

            // Update group balance if transaction is approved
            if (updateGroupTransactionModel.Status == TransactionStatus.APPROVED)
            {
                if (transaction.Type == TransactionType.INCOME)
                {
                    groupFund.CurrentBalance += transaction.Amount;
                }
                else
                {
                    groupFund.CurrentBalance -= transaction.Amount;
                }
                groupFund.UpdatedBy = currentUser.Email;
                _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            }

            // Update transaction in repository
            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);

            // push noti to member in group


            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_RESPONSE_SUCCESS,
                Data = _mapper.Map<TransactionModel>(transaction)
            };
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
    }
}
