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

namespace MoneyEz.Services.Services.Implements
{
    public class GroupFundsService : IGroupFundsService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;

        public GroupFundsService(IMapper mapper, IUnitOfWork unitOfWork, IClaimsService claimsService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
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
                Status = CommonsStatus.ACTIVE,
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

        public async Task<BaseResultModel> GetAllGroupFunds()
        {
            // Get all groupFunds from the repository
            var groupFunds = await _unitOfWork.GroupFundLogRepository.GetAllAsync();
            // Return a success result with the groupFunds
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = groupFunds,
                Message = MessageConstants.GROUP_GET_ALL_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> DisbandGroupAsync(Guid groupId)
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

            // Check if the current user is the leader of the group
            var currentUser = _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail).Result;

            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_DISBAND_FORBIDDEN_MESSAGE
                };
            }

            // Check if the group fund has any transactions
            var transactions = await _unitOfWork.TransactionRepository.GetByIdAsync(groupId);
            if (transactions == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = MessageConstants.GROUP_DISBAND_FAIL_MESSAGE
                };
            }

            // Update the status of the group fund to disbanded
            groupFund.Status = CommonsStatus.INACTIVE;

            // Add a log entry for the disband action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangeDescription = "Group disbanded",
                ChangedAt = CommonUtils.GetCurrentTime(),
                Action = GroupAction.DELETED,
            });

            // Save the changes to the repository
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            _unitOfWork.Save();

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
                    Message = MessageConstants.GROUP_REMOVE_MEMBER_FORBIDDEN_MESSAGE
                };
            }

            // Remove the member from the group
            groupFund.GroupMembers.Remove(memberToRemove);

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
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdAsync(groupId);
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
    }
}