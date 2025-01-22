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

namespace MoneyEz.Services.Services.Implements
{
    public class GroupFundsService : IGroupFundsService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public GroupFundsService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<BaseResultModel> CreateGroupFundsAsync(CreateGroupModel model)
        {
            // Map the incoming model to a GroupFund entity
            var groupEntity = _mapper.Map<GroupFund>(model);

            // Add the new group entity to the repository and save changes
            await _unitOfWork.GroupRepository.AddAsync(groupEntity);
            await _unitOfWork.SaveAsync();

            // Map the model to a new GroupFund entity and set its Id to the one generated for groupEntity
            var groupFund = _mapper.Map<GroupFund>(model);

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
            await _unitOfWork.SaveAsync();

            // Return a success result with the created groupFund
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new GroupFund
                {
                    Name = groupFund.Name,
                    Description = groupFund.Description,
                    CurrentBalance = groupFund.CurrentBalance,
                    Leader = groupFund.Leader,
                    Status = CommonsStatus.ACTIVE,
                    Visibility = VisibilityEnum.PUBLIC,
                },
                Message = MessageConstants.GROUP_CREATE_SUCCESS
            };
        }
    }
}