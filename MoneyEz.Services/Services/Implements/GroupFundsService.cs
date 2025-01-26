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
                UserId = _claimsService.GetCurrentUserId,
                ContributionPercentage = 100, // Assuming the leader contributes 100%
                Role = RoleEnum.ADMIN, // Assuming you have an enum for roles
                Status = CommonsStatus.ACTIVE // Assuming you have an enum for status
            }
            };
            groupFund.GroupFundLogs = new List<GroupFundLog>
            {
                 new GroupFundLog
            {
                ChangeDescription = "Group created",
                ChangedAt = DateTime.UtcNow,
                Group = groupFund,
            }
            };

            // Check if the groupFund is null (which it shouldn't be due to the mapping)
            if (groupFund == null)
            {
                // Return an error result if groupFund is null
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
                };
            }

            // Add the groupFund to the repository and save changes again
            await _unitOfWork.GroupRepository.AddAsync(groupFund);
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
                Message = MessageConstants.GROUP_CREATE_SUCCESS
            };
        }
    }
}