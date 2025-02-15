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
using StackExchange.Redis;
using System.Net.Mail;
using System.Web;
using MoneyEz.Services.BusinessModels.EmailModels;
using MoneyEz.Services.Utils.Email;
using static System.Net.WebRequestMethods;
using MoneyEz.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using System.Text.Json;
using System.Text.Json.Serialization;
using MoneyEz.Services.BusinessModels.UserModels;

namespace MoneyEz.Services.Services.Implements
{
    public class GroupFundsService : IGroupFundsService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly IRedisService _redisService;
        private readonly IMailService _mailService;

        public GroupFundsService(IMapper mapper, IUnitOfWork unitOfWork, IClaimsService claimsService, IRedisService redisService, IMailService mailService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _redisService = redisService;
            _mailService = mailService;
        }

        public async Task<BaseResultModel> CreateGroupFundsAsync(CreateGroupModel model)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Map the model to a new GroupFund entity and set its Id to the one generated for groupEntity
            var groupFund = _mapper.Map<GroupFund>(model);
            groupFund.Status = CommonsStatus.ACTIVE;
            groupFund.Visibility = VisibilityEnum.PRIVATE;

            groupFund.GroupMembers = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = user.Id,
                    ContributionPercentage = 100,
                    Role = RoleGroup.LEADER,
                    Status = GroupMemberStatus.ACTIVE,
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    GroupMemberLogs = new List<GroupMemberLog>
                    {
                        new GroupMemberLog
                        {
                            ChangeDiscription = $"{user.FullName} đã tham gia nhóm",
                            ChangeType = GroupAction.JOINED,
                            CreatedDate = CommonUtils.GetCurrentTime()
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
                    CreatedDate = CommonUtils.GetCurrentTime()
                }
            };
            // Add the groupFund to the repository and save changes again
            await _unitOfWork.GroupFundRepository.AddAsync(groupFund);
            _unitOfWork.Save();
            // Return a success result with the created groupFund
            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Data = new GroupFund
                {
                    Name = groupFund.Name,
                    NameUnsign = StringUtils.ConvertToUnSign(groupFund.Name),
                    Description = groupFund.Description,
                    CurrentBalance = groupFund.CurrentBalance,
                    Status = CommonsStatus.ACTIVE,
                    Visibility = VisibilityEnum.PRIVATE,
                },
                Message = MessageConstants.GROUP_CREATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> GetAllGroupFunds(PaginationParameter paginationParameters)
        {
            // Get all groupFunds from the repository
            var groupFunds = await _unitOfWork.GroupFundRepository.ToPaginationIncludeAsync(paginationParameters);

            var groupFundModels = _mapper.Map<List<GroupFundModel>>(groupFunds);

            var groupPagingResult = PaginationHelper.GetPaginationResult(groupFunds, groupFundModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = groupPagingResult
            };


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
                groupFund.Status = CommonsStatus.INACTIVE;
                _unitOfWork.GroupFundRepository.SoftDeleteAsync(groupFund);

                // Add a log entry for the disband group action
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangeDescription = $"Quỹ đã được đóng bởi {currentUser.FullName}",
                    Action = GroupAction.DELETED,
                    CreatedDate = CommonUtils.GetCurrentTime(),
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

        public async Task<BaseResultModel> RemoveMemberAsync(Guid groupId, Guid memberId)
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

            // Include GroupMembers
            groupFund = await _unitOfWork.GroupFundRepository
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
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_REMOVE_MEMBER_FORBIDDEN
                };
            }

            // Find the member to be removed
            var memberToRemove = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == memberId);
            if (memberToRemove == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_MEMBER_NOT_FOUND
                };
            }

            // Check if the member has any transactions
            var memberTransactions = groupFund.Transactions.Any(t => t.UserId == memberId);
            if (memberTransactions)
            {
                // Soft delete: mark the member as inactive
                memberToRemove.Status = GroupMemberStatus.INACTIVE;
                _unitOfWork.GroupMemberRepository.SoftDeleteAsync(memberToRemove);
            }
            else
            {
                // Hard delete: remove the member from the database
                _unitOfWork.GroupMemberRepository.PermanentDeletedAsync(memberToRemove);
            }

            // Add a log entry for the remove member action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = "Member removed",
                Action = GroupAction.DELETED,
                CreatedDate = CommonUtils.GetCurrentTime()
            });

            // Save the changes to the repository
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_REMOVE_MEMBER_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> SetMemberRoleAsync(Guid groupId, Guid memberId, RoleGroup newRole)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(groupId, include: query => query.Include(g => g.GroupMembers));
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
            var memberToUpdate = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == memberId);
            if (memberToUpdate == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_MEMBER_NOT_FOUND
                };
            }

            // Update the member's role
            memberToUpdate.Role = newRole;

            // Add a log entry for the role change action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = $"Member role changed to {newRole}",
                Action = GroupAction.UPDATED,
                CreatedDate = CommonUtils.GetCurrentTime()
            });

            // Save the changes to the repository
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

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

        public async Task<BaseResultModel> InviteMemberAsync(InviteMemberModel inviteMemberModel, string currentEmail)
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
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(currentEmail);
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

            // Generate a raw invitation token
            var rawToken = Guid.NewGuid().ToString();

            // Hash the token using BCrypt
            var hashedToken = StringUtils.HashToken(rawToken);

            var invitationLink = $"https://easymoney.anttravel.online/api/groups/accept-invitation?token={HttpUtility.UrlEncode(hashedToken)}";
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
                Body = $"Bạn đã được {currentUser.FullName} mời vào nhóm {groupFund.Name}. Ấn vào link để tham gia: {invitationLink}"
            };

            // send mail
            await _mailService.SendEmailAsync(newEmail);

            // Add the member to the group with a pending status
            var pendingMember = new GroupMember
            {
                UserId = inviteUser.Id,
                ContributionPercentage = 0,
                Role = RoleGroup.MEMBER,
                Status = GroupMemberStatus.PENDING,
                CreatedDate = CommonUtils.GetCurrentTime()
            };
            groupFund.GroupMembers.Add(pendingMember);

            // Add a log entry for the invite member action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = $"{inviteUser.FullName} đã được mời vào nhóm qua Email",
                Action = GroupAction.INVITED,
                CreatedDate = CommonUtils.GetCurrentTime()
            });

            // Save the changes to the repository
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_INVITE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> AcceptInvitationAsync(string token)
        {
            // Retrieve the invitation email from Redis
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

            // Retrieve the user by email
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(groupInviteRedisModel.UserId);
            if (user == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.ACCOUNT_NOT_EXIST
                };
            }

            // Update the member status to active
            var pendingMember = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == user.Id && member.Status == GroupMemberStatus.PENDING);
            if (pendingMember != null)
            {
                pendingMember.Status = GroupMemberStatus.ACTIVE;
                pendingMember.UpdatedDate = CommonUtils.GetCurrentTime();
                pendingMember.GroupMemberLogs = new List<GroupMemberLog>
                {
                    new GroupMemberLog
                    {
                        ChangeDiscription = $"{user.FullName} đã tham gia nhóm",
                        ChangeType = GroupAction.JOINED,
                        CreatedDate = CommonUtils.GetCurrentTime()
                    }
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

            // Save the changes to the repository
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // Remove the invitation token from Redis
            await _redisService.RemoveAsync(token);

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> GetGroupFundById(Guid groupId)
        {
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(groupId, include: q => q.Include(c => c.GroupMembers));
            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<GroupFundModel>(groupFund)
            };
        }
    }
}
