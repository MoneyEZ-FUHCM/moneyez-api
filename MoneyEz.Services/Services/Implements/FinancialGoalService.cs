using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.FinancialGoalModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class FinancialGoalService : IFinancialGoalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public FinancialGoalService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        #region Personal
        public async Task<BaseResultModel> AddPersonalFinancialGoalAsync(AddPersonalFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            // UserSpendingModel đang hoạt động
            var activeSpendingModels = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id && usm.EndDate > CommonUtils.GetCurrentTime() && !usm.IsDeleted
            );

            if (!activeSpendingModels.Any())
            {
                throw new DefaultException("User does not have an active spending model.", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);
            }

            var activeSpendingModelId = activeSpendingModels
                .Select(usm => usm.SpendingModelId)
                .FirstOrDefault();

            if (activeSpendingModelId == Guid.Empty)
            {
                throw new DefaultException("Invalid spending model data.", MessageConstants.INVALID_SPENDING_MODEL);
            }

            // SpendingModelCategory của SpendingModel hiện tại
            var spendingModelCategories = await _unitOfWork.SpendingModelCategoryRepository.GetByConditionAsync(
                filter: smc => smc.SpendingModelId == activeSpendingModelId
            );

            if (!spendingModelCategories.Any())
            {
                throw new DefaultException("No categories found for the current spending model.", MessageConstants.SPENDING_MODEL_HAS_NO_CATEGORIES);
            }

            // `CategorySubcategories` liên quan
            var categoryIds = spendingModelCategories.Select(smc => smc.CategoryId).ToList();
            var categorySubcategories = await _unitOfWork.CategorySubcategoryRepository.GetByConditionAsync(
                filter: cs => categoryIds.Contains(cs.CategoryId)
            );

            if (!categorySubcategories.Any())
            {
                throw new DefaultException("No subcategories found in the associated categories.", MessageConstants.SPENDING_MODEL_HAS_NO_SUBCATEGORIES);
            }

            // `SubcategoryId` có tồn tại trong danh sách CategorySubcategories không
            var subcategoryExists = categorySubcategories.Any(cs => cs.SubcategoryId == model.SubcategoryId);

            if (!subcategoryExists)
            {
                throw new DefaultException("The selected subcategory does not exist in the current spending model.", MessageConstants.SUBCATEGORY_NOT_IN_SPENDING_MODEL);
            }

            // Subcategory này đã có Goal chưa
            var existingGoals = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.UserId == user.Id && fg.SubcategoryId == model.SubcategoryId
            );

            if (existingGoals.Any())
            {
                throw new DefaultException("A financial goal already exists for this subcategory.", MessageConstants.SUBCATEGORY_ALREADY_HAS_GOAL);
            }

            if (model.TargetAmount <= 0)
            {
                throw new DefaultException("Target amount must be greater than 0.", MessageConstants.INVALID_TARGET_AMOUNT);
            }

            if (model.TargetAmount <= model.CurrentAmount)
            {
                throw new DefaultException("Target amount must be greater than current amount.", MessageConstants.INVALID_TARGET_AMOUNT);
            }

            if (model.Deadline <= CommonUtils.GetCurrentTime())
            {
                throw new DefaultException("Deadline must be a future date.", MessageConstants.INVALID_DEADLINE);
            }

            var financialGoal = _mapper.Map<FinancialGoal>(model);
            financialGoal.UserId = user.Id;
            financialGoal.CreatedDate = CommonUtils.GetCurrentTime();

            await _unitOfWork.FinancialGoalRepository.AddAsync(financialGoal);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = "Financial goal created successfully."
            };
        }
        public async Task<BaseResultModel> GetPersonalFinancialGoalsAsync(PaginationParameter paginationParameter)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoals = await _unitOfWork.FinancialGoalRepository.ToPaginationIncludeAsync(
                paginationParameter,
                filter: fg => fg.UserId == user.Id && fg.GroupId == null
            );

            var mappedGoals = _mapper.Map<List<PersonalFinancialGoalModel>>(financialGoals);

            var paginatedResult = new Pagination<PersonalFinancialGoalModel>(
                mappedGoals,
                financialGoals.TotalCount,
                financialGoals.CurrentPage,
                financialGoals.PageSize
            );

            var metaData = new
            {
                financialGoals.TotalCount,
                financialGoals.PageSize,
                financialGoals.CurrentPage,
                financialGoals.TotalPages,
                financialGoals.HasNext,
                financialGoals.HasPrevious
            };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new
                {
                    Data = paginatedResult,
                    MetaData = metaData
                }
            };
        }
        public async Task<BaseResultModel> GetPersonalFinancialGoalByIdAsync(GetPersonalFinancialGoalDetailModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.GoalId && fg.UserId == user.Id && fg.GroupId == null
            );

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var mappedGoal = _mapper.Map<PersonalFinancialGoalModel>(financialGoal.First());

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = mappedGoal
            };
        }
        public async Task<BaseResultModel> UpdatePersonalFinancialGoalAsync(UpdatePersonalFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.Id && fg.UserId == user.Id && fg.GroupId == null
            );

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var goalToUpdate = financialGoal.First();

            // TargetAmount phải lớn hơn 0 và lớn hơn CurrentAmount
            if (model.TargetAmount <= 0)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Target amount must be greater than 0."
                };
            }

            if (model.TargetAmount < goalToUpdate.CurrentAmount)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Target amount cannot be less than the current amount."
                };
            }

            if (model.Deadline <= CommonUtils.GetCurrentTime())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_DEADLINE,
                    Message = "Deadline must be a future date."
                };
            }

            //CurrentAmount sẽ được cập nhật theo từng transaction
            goalToUpdate.Name = model.Name;
            goalToUpdate.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            goalToUpdate.TargetAmount = model.TargetAmount;
            goalToUpdate.Deadline = model.Deadline;
            goalToUpdate.UpdatedDate = CommonUtils.GetCurrentTime();

            _unitOfWork.FinancialGoalRepository.UpdateAsync(goalToUpdate);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Financial goal updated successfully."
            };
        }
        public async Task<BaseResultModel> DeletePersonalFinancialGoalAsync(DeleteFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.Id && fg.UserId == user.Id && fg.GroupId == null
            );

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var goalToDelete = financialGoal.First();

            if (goalToDelete.TargetAmount > goalToDelete.CurrentAmount || goalToDelete.Deadline > CommonUtils.GetCurrentTime())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.FINANCIAL_GOAL_CANNOT_BE_DELETED,
                    Message = "Cannot delete a financial goal that is not yet completed or past its deadline."
                };
            }

            _unitOfWork.FinancialGoalRepository.SoftDeleteAsync(goalToDelete);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Financial goal deleted successfully."
            };
        }


        #endregion Personal

        #region Group
        public async Task<BaseResultModel> AddGroupFinancialGoalAsync(AddGroupFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            // User có trong nhóm không và có phải Leader/Mod không
            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId && gm.UserId == user.Id
            );

            if (!groupMember.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_IN_GROUP,
                    Message = "User is not a member of this group."
                };
            }

            var userRole = groupMember.First().Role;
            if (userRole != RoleGroup.LEADER && userRole != RoleGroup.MOD)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_AUTHORIZED,
                    Message = "Only group leaders or moderators can create financial goals for the group."
                };
            }

            // Nhóm đã có Goal chưa
            var existingGoals = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.GroupId == model.GroupId
            );

            if (existingGoals.Any())
            {
                var activeGoal = existingGoals.First();
                if (activeGoal.Deadline > CommonUtils.GetCurrentTime() || activeGoal.CurrentAmount < activeGoal.TargetAmount)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        ErrorCode = MessageConstants.GROUP_ALREADY_HAS_GOAL,
                        Message = "The group already has an active financial goal that is not yet completed or past its deadline."
                    };
                }
            }

            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdAsync(model.GroupId)
                ?? throw new NotExistException(MessageConstants.GROUP_NOT_FOUND);

            if (model.TargetAmount <= 0)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Target amount must be greater than 0."
                };
            }

            if (model.TargetAmount <= model.CurrentAmount)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Target amount must be greater than current amount."
                };
            }

            // Lấy `CurrentBalance` từ `GroupFund`
            decimal currentBalance = groupFund.CurrentBalance;

            if (model.CurrentAmount > currentBalance)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INSUFFICIENT_GROUP_FUNDS,
                    Message = "The group's current balance is insufficient for this financial goal."
                };
            }

            if (model.Deadline <= CommonUtils.GetCurrentTime())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_DEADLINE,
                    Message = "Deadline must be a future date."
                };
            }

            var financialGoal = new FinancialGoal
            {
                UserId = user.Id,
                GroupId = model.GroupId,
                Name = model.Name,
                NameUnsign = StringUtils.ConvertToUnSign(model.Name),
                TargetAmount = model.TargetAmount,
                CurrentAmount = model.CurrentAmount > 0 ? model.CurrentAmount : currentBalance, // Nếu CurrentAmount không được truyền, lấy từ CurrentBalance
                Deadline = model.Deadline,
                CreatedDate = CommonUtils.GetCurrentTime()
            };

            await _unitOfWork.FinancialGoalRepository.AddAsync(financialGoal);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = "Financial goal created successfully for the group."
            };
        }
        public async Task<BaseResultModel> GetGroupFinancialGoalsAsync(GetGroupFinancialGoalsModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            // Kiểm tra xem user có trong nhóm không
            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId && gm.UserId == user.Id
            );

            if (!groupMember.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_IN_GROUP,
                    Message = "User is not a member of this group."
                };
            }

            var financialGoals = await _unitOfWork.FinancialGoalRepository.ToPaginationIncludeAsync(
                new PaginationParameter { PageSize = 10, PageIndex = 1 }, // Hoặc có thể điều chỉnh phân trang
                filter: fg => fg.GroupId == model.GroupId
            );

            var mappedGoals = _mapper.Map<List<GroupFinancialGoalModel>>(financialGoals);

            var paginatedResult = new Pagination<GroupFinancialGoalModel>(
                mappedGoals,
                financialGoals.TotalCount,
                financialGoals.CurrentPage,
                financialGoals.PageSize
            );

            var metaData = new
            {
                financialGoals.TotalCount,
                financialGoals.PageSize,
                financialGoals.CurrentPage,
                financialGoals.TotalPages,
                financialGoals.HasNext,
                financialGoals.HasPrevious
            };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new
                {
                    Data = paginatedResult,
                    MetaData = metaData
                }
            };
        }
        public async Task<BaseResultModel> UpdateGroupFinancialGoalAsync(UpdateGroupFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.Id && fg.GroupId == model.GroupId
            );

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var goalToUpdate = financialGoal.First();

            // User có trong nhóm không và có phải Leader/Mod không
            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId && gm.UserId == user.Id
            );

            if (!groupMember.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_IN_GROUP,
                    Message = "User is not a member of this group."
                };
            }

            var userRole = groupMember.First().Role;
            if (userRole != RoleGroup.LEADER && userRole != RoleGroup.MOD)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_AUTHORIZED,
                    Message = "Only group leaders or moderators can update financial goals."
                };
            }

            if (model.TargetAmount <= 0)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Target amount must be greater than 0."
                };
            }

            if (model.TargetAmount < goalToUpdate.CurrentAmount)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Target amount cannot be less than the current amount."
                };
            }

            if (model.Deadline <= CommonUtils.GetCurrentTime())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_DEADLINE,
                    Message = "Deadline must be a future date."
                };
            }

            goalToUpdate.Name = model.Name;
            goalToUpdate.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            goalToUpdate.TargetAmount = model.TargetAmount;
            goalToUpdate.Deadline = model.Deadline;
            goalToUpdate.UpdatedDate = CommonUtils.GetCurrentTime();

            _unitOfWork.FinancialGoalRepository.UpdateAsync(goalToUpdate);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Financial goal updated successfully."
            };
        }
        public async Task<BaseResultModel> DeleteGroupFinancialGoalAsync(DeleteFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.Id && fg.GroupId != null
            );

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var goalToDelete = financialGoal.First();

            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == goalToDelete.GroupId && gm.UserId == user.Id
            );

            if (!groupMember.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_IN_GROUP,
                    Message = "User is not a member of this group."
                };
            }

            var userRole = groupMember.First().Role;
            if (userRole != RoleGroup.LEADER)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_AUTHORIZED,
                    Message = "Only group leaders can delete financial goals."
                };
            }

            if (goalToDelete.TargetAmount > goalToDelete.CurrentAmount || goalToDelete.Deadline > CommonUtils.GetCurrentTime())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.FINANCIAL_GOAL_CANNOT_BE_DELETED,
                    Message = "Cannot delete a financial goal that is not yet completed or past its deadline."
                };
            }

            _unitOfWork.FinancialGoalRepository.SoftDeleteAsync(goalToDelete);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Financial goal deleted successfully."
            };
        }
        public async Task<BaseResultModel> GetGroupFinancialGoalByIdAsync(GetGroupFinancialGoalDetailModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            // user có trong nhóm không
            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId && gm.UserId == user.Id
            );

            if (!groupMember.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_IN_GROUP,
                    Message = "User is not a member of this group."
                };
            }

            // chỉ lấy Goal thuộc nhóm
            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.GoalId && fg.GroupId == model.GroupId
            );

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var mappedGoal = _mapper.Map<GroupFinancialGoalModel>(financialGoal.First());

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = mappedGoal
            };
        }
        #endregion Group
    }
}