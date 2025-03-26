using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.FinancialGoalModels;
using MoneyEz.Services.BusinessModels.FinancialGoalModels.CreatePersonnalGoal;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.TransactionModels;
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
        private readonly INotificationService _notificationService;
        private readonly ITransactionNotificationService _transactionNotificationService;
        private readonly IGoalPredictionService _goalPredictionService;

        public FinancialGoalService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IClaimsService claimsService,
            INotificationService notificationService,
            ITransactionNotificationService transactionNotificationService,
            IGoalPredictionService goalPredictionService)

        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
            _notificationService = notificationService;
            _transactionNotificationService = transactionNotificationService;
            _goalPredictionService = goalPredictionService;
        }

        #region Personal
        public async Task<BaseResultModel> AddPersonalFinancialGoalAsync(AddPersonalFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var activeSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(user.Id);

            if (activeSpendingModel == null)
            {
                throw new DefaultException("Bạn chưa có mô hình chi tiêu đang hoạt động.", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);
            }

            var spendingModelCategories = await _unitOfWork.SpendingModelCategoryRepository.GetByConditionAsync(
                filter: smc => smc.SpendingModelId == activeSpendingModel.SpendingModelId
            );

            if (!spendingModelCategories.Any())
            {
                throw new DefaultException("Mô hình chi tiêu hiện tại không có danh mục nào.",
                    MessageConstants.SPENDING_MODEL_HAS_NO_CATEGORIES);
            }

            var categoryIds = spendingModelCategories.Select(smc => smc.CategoryId).ToList();
            var categorySubcategories = await _unitOfWork.CategorySubcategoryRepository.GetByConditionAsync(
                filter: cs => categoryIds.Contains(cs.CategoryId),
                include: cs => cs.Include(cs => cs.Subcategory)
            );

            if (!categorySubcategories.Any())
            {
                throw new DefaultException("Không tìm thấy danh mục con nào trong mô hình chi tiêu hiện tại.",
                    MessageConstants.SPENDING_MODEL_HAS_NO_SUBCATEGORIES);
            }

            if (!categorySubcategories.Any(cs => cs.SubcategoryId == model.SubcategoryId))
            {
                throw new DefaultException("Danh mục con đã chọn không thuộc mô hình chi tiêu hiện tại.",
                    MessageConstants.SUBCATEGORY_NOT_IN_SPENDING_MODEL);
            }

            var availableBudget = await CalculateMaximumTargetAmountSubcategory(model.SubcategoryId, user.Id);
            if (model.TargetAmount > availableBudget)
            {
                throw new DefaultException($"Số tiền mục tiêu không được lớn hơn số tiền hiện có ({availableBudget}).",
                    MessageConstants.INVALID_TARGET_AMOUNT);
            }

            var existingGoals = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.UserId == user.Id
                            && fg.SubcategoryId == model.SubcategoryId
                            && fg.Status == FinancialGoalStatus.ACTIVE
                            && !fg.IsDeleted
                            && fg.GroupId == null
            );

            if (existingGoals.Any())
            {
                throw new DefaultException("Bạn đã có mục tiêu tài chính đang hoạt động cho tiểu mục này.",
                    MessageConstants.SUBCATEGORY_ALREADY_HAS_GOAL);
            }

            if (model.TargetAmount <= 0)
            {
                throw new DefaultException("Số tiền mục tiêu phải lớn hơn 0.", MessageConstants.INVALID_TARGET_AMOUNT);
            }

            if (model.TargetAmount <= model.CurrentAmount)
            {
                throw new DefaultException("Số tiền mục tiêu phải lớn hơn số tiền hiện có.",
                    MessageConstants.INVALID_TARGET_AMOUNT);
            }

            var financialGoal = _mapper.Map<FinancialGoal>(model);
            financialGoal.UserId = user.Id;
            financialGoal.Status = FinancialGoalStatus.ACTIVE; // Mặc định ACTIVE
            financialGoal.ApprovalStatus = ApprovalStatus.APPROVED; // Mặc định APPROVED
            financialGoal.StartDate = activeSpendingModel.StartDate.Value;
            financialGoal.Deadline = activeSpendingModel.EndDate.Value;
            financialGoal.Name = categorySubcategories.First(cs => cs.SubcategoryId == model.SubcategoryId).Subcategory.Name;
            financialGoal.NameUnsign = StringUtils.ConvertToUnSign(financialGoal.Name);
            financialGoal.CreatedBy = user.Email;

            // scan existing transactions to calculate current amount
            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id
                            && t.SubcategoryId == model.SubcategoryId
                            && t.CreatedDate >= activeSpendingModel.StartDate
                            && t.CreatedDate <= activeSpendingModel.EndDate
                            && t.Status == TransactionStatus.APPROVED
                            && !t.IsDeleted
            );

            financialGoal.CurrentAmount = transactions.Sum(t => t.Amount);

            await _unitOfWork.FinancialGoalRepository.AddAsync(financialGoal);
            await _unitOfWork.SaveAsync();

            if (model.TargetAmount <= financialGoal.CurrentAmount)
            {
                financialGoal.Status = FinancialGoalStatus.ARCHIVED;
                financialGoal.ApprovalStatus = ApprovalStatus.APPROVED;

                await _transactionNotificationService.NotifyGoalAchievedAsync(user, financialGoal);
                _unitOfWork.FinancialGoalRepository.UpdateAsync(financialGoal);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Tạo mục tiêu tài chính thành công."
                };
            }
            else
            {
                await _transactionNotificationService.NotifyGoalProgressTrackingAsync(user, financialGoal);
                _unitOfWork.FinancialGoalRepository.UpdateAsync(financialGoal);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Tạo mục tiêu tài chính thành công."
                };
            }
        }
        public async Task<BaseResultModel> GetPersonalFinancialGoalsAsync(PaginationParameter paginationParameter, FinancialGoalFilter filter)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoals = await _unitOfWork.FinancialGoalRepository.GetPersonalFinancialGoalsFilterAsync(
                user.Id,
                paginationParameter,
                filter,
                include: fg => fg.Include(fg => fg.Subcategory)
            );

            var mappedGoals = _mapper.Map<List<PersonalFinancialGoalModel>>(financialGoals);

            var paginatedResult = PaginationHelper.GetPaginationResult(financialGoals, mappedGoals);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = paginatedResult
            };
        }
        public async Task<BaseResultModel> GetPersonalFinancialGoalByIdAsync(Guid id)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == id
                            && fg.UserId == user.Id
                            && fg.GroupId == null,
                include: fg => fg.Include(fg => fg.Subcategory)
            );

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var goal = financialGoal.First();
            var mappedGoal = _mapper.Map<PersonalFinancialGoalModel>(goal);

            // Add prediction data
            mappedGoal.Prediction = await _goalPredictionService.PredictGoalCompletion(id, goal.IsSaving);

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
                filter: fg => fg.Id == model.Id
                            && fg.UserId == user.Id
                            && fg.GroupId == null
                            && fg.Status == FinancialGoalStatus.ACTIVE
                            && !fg.IsDeleted
            );

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var goalToUpdate = financialGoal.First();

            if (model.TargetAmount <= 0)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Số tiền mục tiêu phải lớn hơn 0."
                };
            }

            if (model.TargetAmount < goalToUpdate.CurrentAmount)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Số tiền mục tiêu không được nhỏ hơn số tiền hiện tại."
                };
            }

            if (model.Deadline <= CommonUtils.GetCurrentTime())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_DEADLINE,
                    Message = "Ngày hoàn thành mục tiêu phải là ngày trong tương lai."
                };
            }

            goalToUpdate.Name = model.Name;
            goalToUpdate.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            goalToUpdate.TargetAmount = model.TargetAmount;
            goalToUpdate.Deadline = model.Deadline;
            goalToUpdate.UpdatedDate = CommonUtils.GetCurrentTime();
            goalToUpdate.Status = FinancialGoalStatus.ACTIVE;
            goalToUpdate.ApprovalStatus = ApprovalStatus.APPROVED;

            _unitOfWork.FinancialGoalRepository.UpdateAsync(goalToUpdate);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Cập nhật mục tiêu tài chính thành công."
            };
        }
        public async Task<BaseResultModel> DeletePersonalFinancialGoalAsync(DeleteFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var goal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.Id
                            && fg.UserId == user.Id
                            && fg.GroupId == null
                            && !fg.IsDeleted
            );

            if (!goal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var financialGoal = goal.First();

            // Xóa mềm với trạng thái và approval mặc định
            financialGoal.IsDeleted = true;
            financialGoal.Status = FinancialGoalStatus.ARCHIVED;
            financialGoal.ApprovalStatus = ApprovalStatus.APPROVED;  // Đặt mặc định theo yêu cầu
            financialGoal.UpdatedDate = CommonUtils.GetCurrentTime();

            _unitOfWork.FinancialGoalRepository.UpdateAsync(financialGoal);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Personal financial goal deleted successfully."
            };
        }

        public async Task<BaseResultModel> GetUserLimitBugdetSubcategoryAsync(Guid subcategoryId)
        {
            // Get current user
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);

            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(subcategoryId)
                ?? throw new NotExistException("", MessageConstants.SUBCATEGORY_NOT_FOUND);

            var availableBudget = await CalculateMaximumTargetAmountSubcategory(subcategoryId, user.Id);

            // Create and return the result model
            var limitModel = new LimitBugdetSubcategoriesModel
            {
                SubcategoryId = subcategoryId,
                SubcategoryName = subcategory.Name,
                LimitBudget = availableBudget
            };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = limitModel
            };
        }

        public async Task<BaseResultModel> GetUserTransactionsGoalAsync(Guid goalId, PaginationParameter paginationParameter)
        {
            // Get current user
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            // Get and validate financial goal
            var goal = await _unitOfWork.FinancialGoalRepository.GetByIdIncludeAsync(
                goalId,
                filter: fg => fg.UserId == user.Id
                    && !fg.IsDeleted,
                include: query => query.Include(fg => fg.Subcategory)
            );

            if (goal == null)
            {
                throw new NotExistException("", MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            // Get current spending model
            var currentModel = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id
                    && usm.EndDate > CommonUtils.GetCurrentTime()
                    && usm.Status == UserSpendingModelStatus.ACTIVE
                    && !usm.IsDeleted
            );

            if (!currentModel.Any())
            {
                throw new NotExistException(MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);
            }

            var userSpendingModel = currentModel.First();

            // Get transactions 
            var transactions = await _unitOfWork.TransactionsRepository.GetTransactionsFilterAsync(
                paginationParameter,
                new TransactionFilter
                {
                    SubcategoryId = goal.SubcategoryId,
                },
                condition: t => t.UserId == user.Id,
                include: query => query
                    .Include(t => t.Subcategory)
            );

            // Map to transaction models
            var transactionModels = _mapper.Map<List<TransactionModel>>(transactions);

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityNameAsync(EntityName.TRANSACTION.ToString());
            foreach (var transactionModel in transactionModels)
            {
                var transactionImage = images.Where(i => i.EntityId == transactionModel.Id).ToList();
                transactionModel.Images = images.Select(i => i.ImageUrl).ToList();
            }

            // Create paginated result
            var paginatedResult = PaginationHelper.GetPaginationResult(transactions, transactionModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = paginatedResult
            };
        }

        public async Task<BaseResultModel> GetUserFinancialGoalBySpendingModelAsync(Guid userSpendingModelId, PaginationParameter paginationParameter, FinancialGoalFilter filter)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var userSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetByIdAsync(userSpendingModelId);
            if (userSpendingModel == null)
            {
                throw new NotExistException("", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            if (userSpendingModel.UserId != user.Id)
            {
                throw new DefaultException("", MessageConstants.USER_SPENDING_MODEL_ACCESS_DENY);
            }

            var financialGoals = await _unitOfWork.FinancialGoalRepository.GetPersonalFinancialGoalsFilterAsync(
                user.Id,
                paginationParameter,
                filter,
                condition: fg =>  fg.StartDate == userSpendingModel.StartDate
                            && fg.Deadline == userSpendingModel.EndDate,
                include: fg => fg.Include(fg => fg.Subcategory)
            );

            var financialGoalModels = _mapper.Map<List<PersonalFinancialGoalModel>>(financialGoals);

            var result = PaginationHelper.GetPaginationResult(financialGoals, financialGoalModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = result
            };
        }

        public async Task<BaseResultModel> GetAvailableCategoriesCreateGoalPersonalAsync()
        {
            // Get current user
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            // Get active spending model
            var activeSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(user.Id)
                ?? throw new NotExistException("", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);

            // Get spending model categories with their subcategories
            var spendingModelCategories = await _unitOfWork.SpendingModelCategoryRepository.GetByConditionAsync(
                filter: smc => smc.SpendingModelId == activeSpendingModel.SpendingModelId,
                include: query => query
                    .Include(smc => smc.Category)
                        .ThenInclude(c => c.CategorySubcategories)
                            .ThenInclude(cs => cs.Subcategory)
            );

            if (!spendingModelCategories.Any())
            {
                throw new DefaultException(
                    "Mô hình chi tiêu hiện tại không có danh mục nào.",
                    MessageConstants.SPENDING_MODEL_HAS_NO_CATEGORIES
                );
            }

            // Get existing active goals
            var existingGoals = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.UserId == user.Id
                    && fg.Status == FinancialGoalStatus.ACTIVE
                    && !fg.IsDeleted
                    && fg.GroupId == null
                    && fg.StartDate == activeSpendingModel.StartDate
                    && fg.Deadline == activeSpendingModel.EndDate
            );

            var subcategoriesWithGoals = existingGoals.Select(g => g.SubcategoryId.Value).ToHashSet();

            // Create result list
            var availableCategories = new List<AvailableCategoriesModel>();

            foreach (var spendingModelCategory in spendingModelCategories)
            {
                var category = spendingModelCategory.Category;
                if (category == null || !category.CategorySubcategories.Any())
                    continue;

                var categoryModel = new AvailableCategoriesModel
                {
                    CategoryId = category.Id,
                    CategoryCode = category.Code,
                    CategoryName = category.Name,
                    CategoryIcon = category.Icon,
                    Subcategories = new List<AvailableSubcategoriesModel>()
                };

                // Add subcategories without goals
                foreach (var categorySubcategory in category.CategorySubcategories)
                {
                    var subcategory = categorySubcategory.Subcategory;
                    if (subcategory == null) continue;

                    var hasGoal = subcategoriesWithGoals.Contains(subcategory.Id);
                    
                    categoryModel.Subcategories.Add(new AvailableSubcategoriesModel
                    {
                        SubcategoryId = subcategory.Id,
                        SubcategoryCode = subcategory.Code,
                        SubcategoryName = subcategory.Name,
                        SubcategoryIcon = subcategory.Icon,
                        Status = hasGoal ? "HAS_GOAL" : "AVAILABLE"
                    });
                }

                if (categoryModel.Subcategories.Any())
                {
                    availableCategories.Add(categoryModel);
                }
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = availableCategories
            };
        }
        #endregion Personal

        #region Group
        public async Task<BaseResultModel> AddGroupFinancialGoalAsync(AddGroupFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId && gm.UserId == user.Id);

            if (!groupMember.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_IN_GROUP,
                    Message = "Bạn không phải là thành viên của nhóm này."
                };
            }

            var userRole = groupMember.First().Role;

            if (userRole == RoleGroup.MEMBER)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_AUTHORIZED,
                    Message = "Chỉ có trưởng nhóm hoặc quản trị viên mới có quyền tạo mục tiêu tài chính cho nhóm."
                };
            }

            var existingGoals = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.GroupId == model.GroupId && fg.Status == FinancialGoalStatus.ACTIVE);

            if (existingGoals.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.GROUP_ALREADY_HAS_GOAL,
                    Message = "Nhóm này đã có mục tiêu tài chính đang hoạt động."
                };
            }

            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdAsync(model.GroupId)
                ?? throw new NotExistException(MessageConstants.GROUP_NOT_FOUND);

            if (model.TargetAmount <= 0)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Số tiền mục tiêu phải lớn hơn 0."
                };
            }

            if (model.TargetAmount <= model.CurrentAmount)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Số tiền mục tiêu phải lớn hơn số tiền hiện tại."
                };
            }

            if (model.CurrentAmount > groupFund.CurrentBalance)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INSUFFICIENT_GROUP_FUNDS,
                    Message = "Số dư hiện tại của nhóm không đủ để khởi tạo mục tiêu này."
                };
            }

            if (model.Deadline <= CommonUtils.GetCurrentTime())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_DEADLINE,
                    Message = "Ngày hoàn thành mục tiêu phải là ngày trong tương lai."
                };
            }

            var financialGoal = new FinancialGoal
            {
                UserId = user.Id,
                GroupId = model.GroupId,
                Name = model.Name,
                NameUnsign = StringUtils.ConvertToUnSign(model.Name),
                TargetAmount = model.TargetAmount,
                CurrentAmount = model.CurrentAmount > 0 ? model.CurrentAmount : groupFund.CurrentBalance,
                Deadline = model.Deadline,
                CreatedDate = CommonUtils.GetCurrentTime(),
            };

            if (userRole == RoleGroup.LEADER)
            {
                financialGoal.Status = FinancialGoalStatus.ACTIVE;
                financialGoal.ApprovalStatus = ApprovalStatus.APPROVED;

                await NotifyGroupMembers(financialGoal, user, "created");
            }
            else if (userRole == RoleGroup.MOD)
            {
                financialGoal.Status = FinancialGoalStatus.PENDING;
                financialGoal.ApprovalStatus = ApprovalStatus.PENDING;

                await NotifyGroupLeaderApprovalRequest(financialGoal, user, "create", "CREATE");
            }

            await _unitOfWork.FinancialGoalRepository.AddAsync(financialGoal);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = (userRole == RoleGroup.LEADER)
                    ? "Tạo mục tiêu tài chính thành công."
                    : "Tạo mục tiêu tài chính thành công và đang chờ trưởng nhóm phê duyệt."
            };
        }
        public async Task<BaseResultModel> GetGroupFinancialGoalsAsync(GetGroupFinancialGoalsModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId && gm.UserId == user.Id
            );

            if (!groupMember.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_IN_GROUP,
                    Message = "Bạn không phải là thành viên của nhóm này."
                };
            }

            var userRole = groupMember.First().Role;

            var allowedStatuses = new List<FinancialGoalStatus>();

            if (userRole == RoleGroup.LEADER || userRole == RoleGroup.MOD)
            {
                // Leader và MOD thấy tất cả
                allowedStatuses.AddRange(new[]
                {
                    FinancialGoalStatus.PENDING,
                    FinancialGoalStatus.ACTIVE,
                    FinancialGoalStatus.ARCHIVED
                });
            }
            else
            {
                // Member chỉ thấy mục tiêu đã duyệt hoặc đã lưu trữ
                allowedStatuses.AddRange(new[]
                {
                    FinancialGoalStatus.ACTIVE,
                    FinancialGoalStatus.ARCHIVED
                });
            }

            var financialGoals = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.GroupId == model.GroupId && allowedStatuses.Contains(fg.Status)
            );

            var mappedGoals = _mapper.Map<List<GroupFinancialGoalModel>>(financialGoals);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = mappedGoals
            };
        }
        public async Task<BaseResultModel> UpdateGroupFinancialGoalAsync(UpdateGroupFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.Id
                            && fg.GroupId == model.GroupId
                            && !fg.IsDeleted);

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var goalToUpdate = financialGoal.First();

            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId && gm.UserId == user.Id);

            if (!groupMember.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_IN_GROUP,
                    Message = "Bạn không phải là thành viên của nhóm này."
                };
            }

            var userRole = groupMember.First().Role;

            if (userRole == RoleGroup.MEMBER)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_AUTHORIZED,
                    Message = "Chỉ có trưởng nhóm hoặc quản trị viên mới có quyền cập nhật mục tiêu tài chính."
                };
            }

            if (model.TargetAmount <= 0)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Số tiền mục tiêu phải lớn hơn 0."
                };
            }

            if (model.TargetAmount < goalToUpdate.CurrentAmount)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TARGET_AMOUNT,
                    Message = "Số tiền mục tiêu không được nhỏ hơn số tiền hiện tại."
                };
            }

            if (model.Deadline <= CommonUtils.GetCurrentTime())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_DEADLINE,
                    Message = "Ngày hoàn thành mục tiêu phải là ngày trong tương lai."
                };
            }

            goalToUpdate.Name = model.Name;
            goalToUpdate.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            goalToUpdate.TargetAmount = model.TargetAmount;
            goalToUpdate.Deadline = model.Deadline;
            goalToUpdate.UpdatedDate = CommonUtils.GetCurrentTime();

            if (userRole == RoleGroup.LEADER)
            {
                // Leader update -> duyệt ngay
                goalToUpdate.Status = FinancialGoalStatus.ACTIVE;
                goalToUpdate.ApprovalStatus = ApprovalStatus.APPROVED;

                await NotifyGroupMembers(goalToUpdate, user, "updated");
            }
            else if (userRole == RoleGroup.MOD)
            {
                // MOD update -> cần duyệt lại
                goalToUpdate.Status = FinancialGoalStatus.PENDING;
                goalToUpdate.ApprovalStatus = ApprovalStatus.PENDING;

                await NotifyGroupLeaderApprovalRequest(goalToUpdate, user, "update", "UPDATE");
            }

            _unitOfWork.FinancialGoalRepository.UpdateAsync(goalToUpdate);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = (userRole == RoleGroup.LEADER)
                    ? "Cập nhật mục tiêu tài chính thành công."
                    : "Cập nhật thành công và đang chờ trưởng nhóm phê duyệt."
            };
        }
        public async Task<BaseResultModel> DeleteGroupFinancialGoalAsync(DeleteFinancialGoalModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.Id
                            && fg.GroupId != null
                            && !fg.IsDeleted);

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var goalToDelete = financialGoal.First();

            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == goalToDelete.GroupId
                            && gm.UserId == user.Id
                            && gm.Status == GroupMemberStatus.ACTIVE);

            if (!groupMember.Any())
            {
                throw new DefaultException("You are not a member of this group.", MessageConstants.USER_NOT_IN_GROUP);
            }

            var userRole = groupMember.First().Role;

            if (userRole == RoleGroup.MEMBER)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_AUTHORIZED,
                    Message = "Chỉ có trưởng nhóm hoặc quản trị viên mới có quyền xóa mục tiêu tài chính của nhóm."
                };
            }

            if (userRole == RoleGroup.LEADER)
            {
                // Leader xóa trực tiếp
                goalToDelete.IsDeleted = true;
                goalToDelete.Status = FinancialGoalStatus.ARCHIVED;
                goalToDelete.ApprovalStatus = ApprovalStatus.APPROVED;
                goalToDelete.UpdatedDate = CommonUtils.GetCurrentTime();

                _unitOfWork.FinancialGoalRepository.UpdateAsync(goalToDelete);
                await _unitOfWork.SaveAsync();

                await NotifyGroupMembers(goalToDelete, user, "deleted");

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Group financial goal has been deleted successfully."
                };
            }
            else if (userRole == RoleGroup.MOD)
            {
                // MOD gửi yêu cầu xóa
                goalToDelete.Status = FinancialGoalStatus.PENDING;
                goalToDelete.ApprovalStatus = ApprovalStatus.PENDING;
                goalToDelete.UpdatedDate = CommonUtils.GetCurrentTime();

                _unitOfWork.FinancialGoalRepository.UpdateAsync(goalToDelete);
                await _unitOfWork.SaveAsync();

                await NotifyGroupLeaderApprovalRequest(goalToDelete, user, "delete", "DELETE");

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Delete request for this financial goal has been sent to the group leader for approval."
                };
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status403Forbidden,
                Message = "You are not authorized to delete this goal."
            };
        }
        public async Task<BaseResultModel> GetGroupFinancialGoalByIdAsync(GetGroupFinancialGoalDetailModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId && gm.UserId == user.Id
            );

            if (!groupMember.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_IN_GROUP,
                    Message = "Bạn không phải là thành viên của nhóm này."
                };
            }

            var userRole = groupMember.First().Role;
            var allowedStatuses = new List<FinancialGoalStatus>();

            if (userRole == RoleGroup.LEADER || userRole == RoleGroup.MOD)
            {
                allowedStatuses.AddRange(new[]
                {
                    FinancialGoalStatus.PENDING,
                    FinancialGoalStatus.ACTIVE,
                    FinancialGoalStatus.ARCHIVED
                });
            }
            else
            {
                allowedStatuses.AddRange(new[]
                {
                    FinancialGoalStatus.ACTIVE,
                    FinancialGoalStatus.ARCHIVED
                });
            }

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.GoalId
                            && fg.GroupId == model.GroupId
                            && allowedStatuses.Contains(fg.Status)
            );

            if (!financialGoal.Any())
            {
                throw new NotExistException(MessageConstants.FINANCIAL_GOAL_NOT_FOUND);
            }

            var goal = financialGoal.First();
            
            // Get all group members
            var groupMembers = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId && gm.Status == GroupMemberStatus.ACTIVE,
                include: query => query.Include(gm => gm.User)
            );

            var memberCount = groupMembers.Count();
            if (memberCount == 0)
            {
                throw new DefaultException("No active members found in the group.", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            // Calculate default equal planned contribution percentage
            var defaultPlannedPercentage = 100m / memberCount;

            // Get all transactions related to this financial goal
            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.GroupId == model.GroupId 
                            && t.CreatedDate >= goal.CreatedDate 
                            && t.CreatedDate <= goal.Deadline
            );

            // Calculate member contributions
            var memberContributions = new List<GroupMemberContributionModel>();
            var totalCurrentContributions = transactions.Sum(t => t.Amount);

            foreach (var member in groupMembers)
            {
                // Calculate actual contributions
                var memberTransactions = transactions.Where(t => t.UserId == member.UserId);
                var currentContributionAmount = memberTransactions.Sum(t => t.Amount);

                // Calculate planned amounts
                var plannedContributionPercentage = defaultPlannedPercentage; // Can be customized per member if needed
                var plannedTargetAmount = (goal.TargetAmount * plannedContributionPercentage) / 100;
                
                // Calculate remaining and completion
                var remainingAmount = Math.Max(0, plannedTargetAmount - currentContributionAmount);
                var completionPercentage = plannedTargetAmount > 0 
                    ? Math.Min(100, (currentContributionAmount / plannedTargetAmount) * 100)
                    : 0;

                memberContributions.Add(new GroupMemberContributionModel
                {
                    UserId = member.UserId,
                    FullName = member.User?.FullName ?? "Unknown User",
                    // Actual metrics
                    CurrentContributionAmount = currentContributionAmount,
                    // Planned metrics
                    PlannedContributionPercentage = Math.Round(plannedContributionPercentage, 2),
                    PlannedTargetAmount = Math.Round(plannedTargetAmount, 2),
                    // Progress metrics
                    RemainingAmount = Math.Round(remainingAmount, 2),
                    CompletionPercentage = Math.Round(completionPercentage, 2)
                });
            }

            // Map the goal to the detailed model
            var mappedGoal = _mapper.Map<GroupFinancialGoalDetailModel>(goal);
            mappedGoal.MemberContributions = memberContributions;
            mappedGoal.TotalCurrentAmount = Math.Round(totalCurrentContributions, 2);
            mappedGoal.CompletionPercentage = goal.TargetAmount > 0 
                ? Math.Round((totalCurrentContributions / goal.TargetAmount) * 100, 2)
                : 0;

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = mappedGoal
            };
        }

        public async Task<BaseResultModel> ApproveGroupFinancialGoalAsync(ApproveGroupFinancialGoalRequestModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var groupMember = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == model.GroupId
                            && gm.UserId == user.Id
                            && gm.Status == GroupMemberStatus.ACTIVE);

            if (!groupMember.Any() || groupMember.First().Role != RoleGroup.LEADER)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.USER_NOT_AUTHORIZED,
                    Message = "Chỉ trưởng nhóm mới có quyền phê duyệt mục tiêu tài chính."
                };
            }

            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.Id == model.GoalId
                            && fg.GroupId == model.GroupId
                            && fg.Status == FinancialGoalStatus.PENDING);

            if (!financialGoal.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.FINANCIAL_GOAL_NOT_FOUND,
                    Message = "Mục tiêu tài chính không tìm thấy hoặc không cần phê duyệt."
                };
            }

            var goalToApprove = financialGoal.First();

            if (model.IsApproved)
            {
                // Approve - cập nhật trạng thái và bắn thông báo theo actionType
                switch (model.ActionType.ToUpper())
                {
                    case "CREATE":
                        goalToApprove.Status = FinancialGoalStatus.ACTIVE;
                        goalToApprove.ApprovalStatus = ApprovalStatus.APPROVED;
                        await NotifyGroupMembers(goalToApprove, user, "approved", "Mục tiêu tài chính mới đã được duyệt.");
                        break;

                    case "UPDATE":
                        goalToApprove.Status = FinancialGoalStatus.ACTIVE;
                        goalToApprove.ApprovalStatus = ApprovalStatus.APPROVED;
                        await NotifyGroupMembers(goalToApprove, user, "approved", "Mục tiêu tài chính đã được cập nhật và duyệt.");
                        break;

                    case "DELETE":
                        goalToApprove.IsDeleted = true;
                        goalToApprove.Status = FinancialGoalStatus.ARCHIVED;
                        goalToApprove.ApprovalStatus = ApprovalStatus.APPROVED;
                        await NotifyGroupMembers(goalToApprove, user, "deleted", "Mục tiêu tài chính đã được duyệt xóa.");
                        break;

                    default:
                        return new BaseResultModel
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Message = "ActionType không hợp lệ."
                        };
                }
            }
            else
            {
                // Reject - cập nhật trạng thái và bắn thông báo theo actionType
                string reason = string.IsNullOrWhiteSpace(model.RejectionReason)
                    ? "Không có lý do cụ thể."
                    : model.RejectionReason;

                switch (model.ActionType.ToUpper())
                {
                    case "CREATE":
                        goalToApprove.IsDeleted = true;
                        goalToApprove.Status = FinancialGoalStatus.ARCHIVED;
                        goalToApprove.ApprovalStatus = ApprovalStatus.REJECTED;
                        await NotifyGroupMembers(goalToApprove, user, "rejected", $"Mục tiêu tài chính mới bị từ chối. Lý do: {reason}");
                        break;

                    case "UPDATE":
                        goalToApprove.Status = FinancialGoalStatus.ACTIVE;
                        goalToApprove.ApprovalStatus = ApprovalStatus.REJECTED;
                        await NotifyGroupMembers(goalToApprove, user, "rejected", $"Cập nhật mục tiêu tài chính bị từ chối. Lý do: {reason}");
                        break;

                    case "DELETE":
                        goalToApprove.Status = FinancialGoalStatus.ACTIVE;
                        goalToApprove.ApprovalStatus = ApprovalStatus.REJECTED;
                        await NotifyGroupMembers(goalToApprove, user, "rejected", $"Yêu cầu xóa mục tiêu tài chính bị từ chối. Lý do: {reason}");
                        break;

                    default:
                        return new BaseResultModel
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Message = "ActionType không hợp lệ."
                        };
                }
            }


            goalToApprove.UpdatedDate = CommonUtils.GetCurrentTime();

            _unitOfWork.FinancialGoalRepository.UpdateAsync(goalToApprove);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = model.IsApproved ? "Phê duyệt thành công." : "Từ chối thành công."
            };
        }

        #endregion Group

        #region notification
        private async Task NotifyGroupMembers(FinancialGoal goal, User actionUser, string actionType, string? customMessage = null)
        {
            var groupMembers = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == goal.GroupId && gm.Status == GroupMemberStatus.ACTIVE);

            string actionMessage = actionType switch
            {
                "created" => "đã tạo",
                "updated" => "đã cập nhật",
                "deleted" => "đã xóa",
                "archived" => "đã lưu trữ",
                "approved" => "đã phê duyệt",
                "rejected" => "đã từ chối",
                _ => "đã cập nhật"
            };

            string message = customMessage ?? $"Mục tiêu tài chính '{goal.Name}' {actionMessage} bởi {actionUser.FullName}.";

            var notification = new Notification
            {
                Title = "Cập nhật mục tiêu tài chính nhóm",
                Message = message,
                Type = NotificationType.GROUP,
                EntityId = goal.Id,
                CreatedDate = CommonUtils.GetCurrentTime()
            };

            await _notificationService.AddNotificationByListUser(
                groupMembers.Select(gm => gm.UserId).ToList(),
                notification);
        }
        private async Task NotifyGroupLeaderApprovalRequest(FinancialGoal goal, User actionUser, string actionType, string actionKey)
        {
            var leaders = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == goal.GroupId
                            && gm.Role == RoleGroup.LEADER
                            && gm.Status == GroupMemberStatus.ACTIVE);

            if (!leaders.Any())
            {
                return;
            }

            string actionMessage = actionType switch
            {
                "create" => "tạo mới",
                "update" => "cập nhật",
                "delete" => "xóa",
                _ => "thay đổi"
            };

            var notification = new Notification
            {
                Title = "Yêu cầu phê duyệt mục tiêu tài chính",
                Message = $"Quản trị viên {actionUser.FullName} vừa yêu cầu {actionMessage} mục tiêu tài chính '{goal.Name}'.",
                Type = NotificationType.GROUP,
                EntityId = goal.Id,
                CreatedDate = CommonUtils.GetCurrentTime(),
                Href = $"/group-goals/{goal.GroupId}/{goal.Id}?action={actionKey}" // kèm action để UI xử lý đẹp
            };

            await _notificationService.AddNotificationByListUser(
                leaders.Select(l => l.UserId).ToList(),
                notification);
        }
        #endregion notification

        private async Task<decimal> CalculateMaximumTargetAmountSubcategory(Guid subcategoryId, Guid userId)
        {
            // Get user's current active spending model
            var currentModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(userId);
            if (currentModel == null)
            {
                throw new NotExistException("", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);
            }

            // Get total income for the spending model period
            var totalIncome = await _unitOfWork.TransactionsRepository.GetToalIncomeByUserSpendingModelAsync(currentModel.Id);

            // Get the category for the subcategory in current spending model
            var category = await _unitOfWork.CategorySubcategoryRepository
                .GetCategoryInCurrentSpendingModel(subcategoryId, currentModel.SpendingModelId.Value)
                ?? throw new DefaultException(
                    "Subcategory này không thuộc danh mục nào trong mô hình chi tiêu hiện tại.",
                    MessageConstants.SUBCATEGORY_NOT_IN_SPENDING_MODEL
                );

            // Check if there's already an active goal for this subcategory
            var existingGoal = await _unitOfWork.FinancialGoalRepository.GetActiveGoalByUserAndSubcategory(userId, subcategoryId);
            if (existingGoal != null)
            {
                throw new DefaultException(
                    "Subcategory này đã có mục tiêu tài chính đang hoạt động.",
                    MessageConstants.SUBCATEGORY_ALREADY_HAS_GOAL
                );
            }

            // Get the spending model category to get the percentage
            var spendingModelCategory = await _unitOfWork.SpendingModelCategoryRepository
                .GetByModelAndCategory(currentModel.SpendingModelId.Value, category.Id)
                ?? throw new DefaultException(
                    "Không tìm thấy thông tin phần trăm cho danh mục này trong mô hình chi tiêu.",
                    MessageConstants.CATEGORY_NOT_FOUND_IN_SPENDING_MODEL
                );

            // Get all subcategories in the same category
            var subcategoryIds = currentModel.SpendingModel.SpendingModelCategories
                .Where(smc => smc.Category?.CategorySubcategories != null && smc.CategoryId == category.Id)
                .SelectMany(smc => smc.Category.CategorySubcategories)
                .Select(sub => sub.SubcategoryId)
                .ToList();

            // Get all active financial goals for subcategories in the same category
            var activeGoals = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.UserId == userId
                    && subcategoryIds.Contains(fg.SubcategoryId.Value)
                    && fg.Status == FinancialGoalStatus.ACTIVE
                    && !fg.IsDeleted
                    && fg.CreatedDate >= currentModel.StartDate
                    && fg.Deadline <= currentModel.EndDate
            );

            // Calculate total target amount already allocated in this category
            var allocatedAmount = activeGoals.Sum(g => g.TargetAmount);

            // Get the subcategory details
            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(subcategoryId)
                ?? throw new NotExistException("", MessageConstants.SUBCATEGORY_NOT_FOUND);

            // Calculate the category's total budget based on income and percentage
            var categoryBudget = totalIncome * (spendingModelCategory.PercentageAmount ?? 0) / 100m;

            // Calculate remaining available budget for new goals
            var availableBudget = Math.Max(0, categoryBudget - allocatedAmount);

            return availableBudget;
        }
    }
}