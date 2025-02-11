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
            // Map the model to a new GroupFund entity and set its Id to the one generated for groupEntity
            var groupFund = _mapper.Map<GroupFund>(model);
            groupFund.GroupMembers = new List<GroupMember>
            {
                 new GroupMember
            {
                UserId = _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail).Result.Id,
                ContributionPercentage = 100,
                Role = RoleGroup.LEADER,
                Status = GroupMemberStatus.ACTIVE,
            }
            };
            groupFund.GroupFundLogs = new List<GroupFundLog>
            {
                 new GroupFundLog
            {
                ChangeDescription = "Group created",
                ChangedAt = CommonUtils.GetCurrentTime(),
                Action = GroupAction.CREATED,

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
            var groupFunds = await _unitOfWork.GroupFundRepository.ToPaginationIncludeAsync(paginationParameters, include: query => query.Include(c => c.GroupMembers));

            var groupFundModels = _mapper.Map<List<GroupFundModel>>(groupFunds);

            var groupPagings = new Pagination<GroupFundModel>(groupFundModels,
                groupFunds.TotalCount,
                groupFunds.CurrentPage,
                groupFunds.PageSize);

            var metaData = new
            {
                groupFunds.TotalCount,
                groupFunds.PageSize,
                groupFunds.CurrentPage,
                groupFunds.TotalPages,
                groupFunds.HasNext,
                groupFunds.HasPrevious
            };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new ModelPaging
                {
                    Data = groupPagings,
                    MetaData = metaData
                }
            };


        }

        public async Task<BaseResultModel> DisbandGroupAsync(Guid groupId)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(groupId, include: query => query.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_FOUND_MESSAGE
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
                    Message = MessageConstants.GROUP_DISBAND_FORBIDDEN_MESSAGE
                };
            }

            var groupFundToDelete = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(groupId);
            // Check if the group has any transactions
            if (groupFund.Transactions.Any())
            {
                // Soft delete: mark the group as inactive
                groupFund.Status = CommonsStatus.INACTIVE;
                _unitOfWork.GroupFundRepository.SoftDeleteAsync(groupFundToDelete);
            }
            else
            {
                // Hard delete: remove the group from the database
                _unitOfWork.GroupFundRepository.SoftDeleteAsync(groupFundToDelete);
            }

            // Add a log entry for the disband group action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = "Group disbanded",
                ChangedAt = CommonUtils.GetCurrentTime(),
                Action = GroupAction.DELETED,
            });

            // Save the changes to the repository
            await _unitOfWork.SaveAsync();

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_DISBAND_SUCCESS_MESSAGE
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
                    Message = MessageConstants.GROUP_NOT_FOUND_MESSAGE
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
                    Message = MessageConstants.GROUP_NOT_FOUND_MESSAGE
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
                    Message = MessageConstants.GROUP_REMOVE_MEMBER_FORBIDDEN_MESSAGE
                };
            }

            // Find the member to be removed
            var memberToRemove = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == memberId);
            if (memberToRemove == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.MEMBER_NOT_FOUND_MESSAGE
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
                ChangedAt = CommonUtils.GetCurrentTime(),
                Action = GroupAction.DELETED,
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
                    Message = MessageConstants.GROUP_NOT_FOUND_MESSAGE
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
                    Message = MessageConstants.GROUP_SET_ROLE_FORBIDDEN_MESSAGE
                };
            }

            // Find the member whose role is to be changed
            var memberToUpdate = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == memberId);
            if (memberToUpdate == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.MEMBER_NOT_FOUND_MESSAGE
                };
            }

            // Update the member's role
            memberToUpdate.Role = newRole;

            // Add a log entry for the role change action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = $"Member role changed to {newRole}",
                ChangedAt = CommonUtils.GetCurrentTime(),
                Action = GroupAction.UPDATED,
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
                    Message = MessageConstants.GROUP_NOT_FOUND_MESSAGE
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
                    Message = MessageConstants.GROUP_NOT_FOUND_MESSAGE
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

            // Generate an invitation link
            var invitationToken = Guid.NewGuid().ToString();
            var invitationLink = $"https://easymoney.anttravel.online/api/groups/{inviteMemberModel.GroupId}/accept-invitation?token={HttpUtility.UrlEncode(invitationToken)}";
            //var invitationLink = $"https://localhost:7262/api/groups/{inviteMemberModel.GroupId}/accept-invitation?token={HttpUtility.UrlEncode(invitationToken)}";

            // Save the invitation token to Redis
            var redisKey = $"group_invitation:{invitationToken}";
            await _redisService.SetAsync(redisKey, inviteMemberModel.Email, TimeSpan.FromDays(1));

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
                Status = GroupMemberStatus.PENDING
            };
            groupFund.GroupMembers.Add(pendingMember);

            // Add a log entry for the invite member action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = $"Invitation sent to {currentEmail} Created ",
                ChangedAt = CommonUtils.GetCurrentTime(),
                Action = GroupAction.INVITED,
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

        public async Task<BaseResultModel> AcceptInvitationAsync(Guid groupId, string token)
        {
            // Retrieve the invitation email from Redis
            var redisKey = $"group_invitation:{token}";
            var email = await _redisService.GetAsync<string>(redisKey);
            if (string.IsNullOrEmpty(email))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = MessageConstants.INVALID_INVITATION_TOKEN_MESSAGE
                };
            }

            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdAsync(groupId);
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_FOUND_MESSAGE
                };
            }

            // Retrieve the user by email
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);
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
            }
            else
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.MEMBER_NOT_FOUND_MESSAGE
                };
            }

            // Add a log entry for the new member
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = $"Member {user.Email} accepted invitation",
                ChangedAt = CommonUtils.GetCurrentTime(),
                Action = GroupAction.JOINED,
            });

            // Save the changes to the repository
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // Remove the invitation token from Redis
            await _redisService.RemoveAsync(redisKey);

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE
            };
        }
    }
}
