using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.ChartModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.SpendingModelModels;
using MoneyEz.Services.BusinessModels.SubcategoryModels;
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
    public class UserSpendingModelService : IUserSpendingModelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;
        private readonly ISpendingModelService _spendingModelService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserSpendingModelService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IClaimsService claimsService,
            ISpendingModelService spendingModelService,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
            _spendingModelService = spendingModelService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResultModel> ChooseSpendingModelAsync(ChooseSpendingModelModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdAsync(model.SpendingModelId)
                ?? throw new NotExistException(MessageConstants.SPENDING_MODEL_NOT_FOUND);

            if (spendingModel.IsTemplate == false)
            {
                throw new DefaultException("Selected spending model is not a template.", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            var startDate = model.StartDate ?? CommonUtils.GetCurrentTime();

            if (startDate < CommonUtils.GetCurrentTime().Date)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.START_DATE_CANNOT_BE_IN_PAST,
                    Message = "Start date cannot be in the past."
                };
            }

            var endDate = CalculateEndDate(startDate, model.PeriodUnit, model.PeriodValue);

            if (endDate < startDate)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.END_DATE_MUST_BE_AFTER_START_DATE,
                    Message = "End date must be after start date."
                };
            }

            var activeModels = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id
                    && usm.EndDate > CommonUtils.GetCurrentTime()
                    && usm.Status == UserSpendingModelStatus.ACTIVE
                    && !usm.IsDeleted
            );

            if (activeModels.Any())
            {
                if (startDate > CommonUtils.GetCurrentTime())
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        ErrorCode = MessageConstants.CANNOT_SELECT_FUTURE_MODEL_WHEN_ACTIVE,
                        Message = "You cannot select a future spending model while your current model is still active."
                    };
                }

                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.USER_ALREADY_HAS_ACTIVE_SPENDING_MODEL,
                    Message = "You already have an active spending model. Please switch or cancel it before choosing a new one."
                };
            }

            var userSpendingModel = new UserSpendingModel
            {
                UserId = user.Id,
                SpendingModelId = spendingModel.Id,
                PeriodUnit = (int)model.PeriodUnit,
                PeriodValue = model.PeriodValue,
                StartDate = startDate,
                EndDate = endDate
            };

            await _unitOfWork.UserSpendingModelRepository.AddAsync(userSpendingModel);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = "Spending model selected successfully."
            };
        }

        public async Task<BaseResultModel> SwitchSpendingModelAsync(SwitchSpendingModelModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var activeModels = await _unitOfWork.UserSpendingModelRepository.ToPaginationIncludeAsync(
                new PaginationParameter { PageSize = 1, PageIndex = 1 },
                filter: usm => usm.UserId == user.Id && usm.EndDate > CommonUtils.GetCurrentTime() && !usm.IsDeleted
            );

            var activeModel = activeModels.FirstOrDefault();

            if (activeModel != null)
            {
                if (model.StartDate > CommonUtils.GetCurrentTime())
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        ErrorCode = MessageConstants.CANNOT_SELECT_FUTURE_MODEL_WHEN_ACTIVE,
                        Message = "You cannot switch to a future spending model while your current model is still active."
                    };
                }

                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.USER_ALREADY_HAS_ACTIVE_SPENDING_MODEL,
                    Message = "You already have an active spending model. Please wait until it ends before switching."
                };
            }

            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdAsync(model.SpendingModelId)
                ?? throw new NotExistException(MessageConstants.SPENDING_MODEL_NOT_FOUND);

            if (spendingModel.IsTemplate == false)
            {
                throw new DefaultException("Selected spending model is not a template.", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            var startDate = model.StartDate ?? CommonUtils.GetCurrentTime().AddDays(1);

            if (startDate < CommonUtils.GetCurrentTime().Date)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.START_DATE_CANNOT_BE_IN_PAST,
                    Message = "Start date cannot be in the past."
                };
            }

            var endDate = CalculateEndDate(startDate, model.PeriodUnit, model.PeriodValue);

            if (endDate < startDate)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.END_DATE_MUST_BE_AFTER_START_DATE,
                    Message = "End date must be after start date."
                };
            }

            var newModel = new UserSpendingModel
            {
                UserId = user.Id,
                SpendingModelId = model.SpendingModelId,
                PeriodUnit = (int)model.PeriodUnit,
                PeriodValue = model.PeriodValue,
                StartDate = startDate,
                EndDate = endDate
            };

            await _unitOfWork.UserSpendingModelRepository.AddAsync(newModel);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Spending model switched successfully."
            };
        }

        public async Task<BaseResultModel> CancelSpendingModelAsync(Guid spendingModelId)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var spendingModels = await _unitOfWork.UserSpendingModelRepository.ToPaginationIncludeAsync(
                new PaginationParameter { PageSize = 1, PageIndex = 1 },
                filter: usm => usm.UserId == user.Id
                            && usm.SpendingModelId == spendingModelId
                            && usm.EndDate > CommonUtils.GetCurrentTime()
                            && !usm.IsDeleted,
                include: query => query.Include(usm => usm.SpendingModel)
                                       .ThenInclude(sm => sm.SpendingModelCategories)
                                       .ThenInclude(smc => smc.Category)
                                       .ThenInclude(c => c.CategorySubcategories)
                                       .ThenInclude(cs => cs.Subcategory)
            );

            var spendingModel = spendingModels.FirstOrDefault();

            if (spendingModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "No active spending model found for cancellation."
                };
            }

            // Lấy danh sách Subcategory
            var subcategoryIds = spendingModel.SpendingModel.SpendingModelCategories
                .SelectMany(smc => smc.Category.CategorySubcategories)
                .Select(cs => cs.Subcategory.Id)
                .Distinct()
                .ToList();

            // Kiểm tra xem có FinancialGoal nào liên quan đến Subcategory trong mô hình này không
            var existingGoals = await _unitOfWork.FinancialGoalRepository.ToPaginationIncludeAsync(
                new PaginationParameter { PageSize = 1, PageIndex = 1 },
                filter: fg => fg.UserId == user.Id && subcategoryIds.Contains(fg.SubcategoryId.Value)
            );

            if (existingGoals.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CANNOT_CANCEL_SPENDING_MODEL_HAS_GOALS,
                    Message = "You cannot cancel this spending model because some subcategories are linked to active financial goals."
                };
            }

            _unitOfWork.UserSpendingModelRepository.SoftDeleteAsync(spendingModel);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Spending model cancelled successfully."
            };
        }

        public async Task<BaseResultModel> GetCurrentSpendingModelAsync()
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var currentModels = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.Status == UserSpendingModelStatus.ACTIVE
                                && usm.UserId == user.Id
                                && usm.EndDate > CommonUtils.GetCurrentTime()
                                && !usm.IsDeleted,
                include: query => query.Include(usm => usm.SpendingModel)
            );

            var currentModel = currentModels.FirstOrDefault();
            if (currentModel == null)
            {
                throw new NotExistException("", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            var currentModelReturn = _mapper.Map<UserSpendingModelModel>(currentModel);

            // Get all transactions for this user where groupId is null
            var allTransactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id &&
                            t.GroupId == null &&
                            t.Status == TransactionStatus.APPROVED
            );

            // Calculate totals for each model
            var modelTransactions = allTransactions.Where(t =>
                t.TransactionDate >= currentModel.StartDate &&
                t.TransactionDate <= currentModel.EndDate);

            currentModelReturn.TotalIncome = modelTransactions
                    .Where(t => t.Type == TransactionType.INCOME)
                    .Sum(t => t.Amount);

            currentModelReturn.TotalExpense = Math.Abs(modelTransactions
                    .Where(t => t.Type == TransactionType.EXPENSE)
                    .Sum(t => t.Amount));


            if (currentModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND
                };
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = currentModelReturn
            };
        }

        public async Task<BaseResultModel> GetUsedSpendingModelByIdAsync(Guid id)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var spendingModels = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id && usm.Id == id,
                include: query => query.Include(usm => usm.SpendingModel)
            );

            var userSpendingModel = spendingModels.FirstOrDefault();

            if (userSpendingModel == null)
            {
                throw new NotExistException("", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            var userSpendingModelReturn = _mapper.Map<UserSpendingModelModel>(userSpendingModel);

            // Get all transactions for this user where groupId is null
            var allTransactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id &&
                            t.GroupId == null &&
                            t.Status == TransactionStatus.APPROVED
            );

            // Calculate totals for each model
            var modelTransactions = allTransactions.Where(t =>
                t.TransactionDate >= userSpendingModel.StartDate &&
                t.TransactionDate <= userSpendingModel.EndDate);

            userSpendingModelReturn.TotalIncome = modelTransactions
                    .Where(t => t.Type == TransactionType.INCOME)
                    .Sum(t => t.Amount);

            userSpendingModelReturn.TotalExpense = Math.Abs(modelTransactions
                    .Where(t => t.Type == TransactionType.EXPENSE)
                    .Sum(t => t.Amount));


            if (userSpendingModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND
                };
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = userSpendingModelReturn
            };
        }

        public async Task<BaseResultModel> GetUsedSpendingModelsPaginationAsync(PaginationParameter paginationParameter)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var usedSpendingModels = await _unitOfWork.UserSpendingModelRepository.ToPaginationIncludeAsync(
                paginationParameter,
                filter: usm => usm.UserId == user.Id,
                include: query => query.Include(usm => usm.SpendingModel),
                orderBy: query => query.OrderByDescending(usm => usm.CreatedDate)
            );

            var mappedResult = _mapper.Map<List<UserSpendingModelHistoryModel>>(usedSpendingModels);

            // Get all transactions for this user where groupId is null
            var allTransactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id &&
                            t.GroupId == null &&
                            t.Status == TransactionStatus.APPROVED
            );

            // Calculate totals for each model
            foreach (var model in mappedResult)
            {
                var modelTransactions = allTransactions.Where(t =>
                    t.TransactionDate >= model.StartDate &&
                    t.TransactionDate <= model.EndDate);

                model.TotalIncome = modelTransactions
                    .Where(t => t.Type == TransactionType.INCOME)
                    .Sum(t => t.Amount);

                model.TotalExpense = Math.Abs(modelTransactions
                    .Where(t => t.Type == TransactionType.EXPENSE)
                    .Sum(t => t.Amount));
            }

            var result = PaginationHelper.GetPaginationResult(usedSpendingModels, mappedResult);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = result
            };
        }

        private DateTime CalculateEndDate(DateTime startDate, PeriodUnit periodUnit, int periodValue)
        {
            return periodUnit switch
            {
                PeriodUnit.DAY => startDate.AddDays(periodValue),
                PeriodUnit.WEEK => startDate.AddDays(periodValue * 7),
                PeriodUnit.MONTH => startDate.AddMonths(periodValue),
                PeriodUnit.YEAR => startDate.AddYears(periodValue),
                _ => throw new
                DefaultException(
                    "Invalid period unit",
                    MessageConstants.INVALID_PERIOD_UNIT
                    )
            };
        }

        public async Task<BaseResultModel> GetChartCurrentSpendingModelAsync()
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var currentModels = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id
                    && usm.EndDate > CommonUtils.GetCurrentTime()
                    && usm.Status == UserSpendingModelStatus.ACTIVE 
                    && !usm.IsDeleted,
                include: query => query
                    .Include(usm => usm.SpendingModel)
                    .ThenInclude(sm => sm.SpendingModelCategories)
                    .ThenInclude(smc => smc.Category)
            );

            var currentModel = currentModels.FirstOrDefault();

            if (currentModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "No active spending model found"
                };
            }

            if (!currentModel.SpendingModel.SpendingModelCategories.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SPENDING_MODEL_HAS_NO_CATEGORIES,
                    Message = "Current spending model has no categories"
                };
            }

            var chartData = new List<ChartSpendingCategoryModel>();
            decimal totalSpent = 0;

            // Get total income for the spending model period
            decimal totalIncome = await _unitOfWork.TransactionsRepository.GetTotalIncomeAsync(
                userId: user.Id,
                groupId: null,
                startDate: currentModel.StartDate.Value,
                endDate: currentModel.EndDate.Value
            );

            // Get all transactions within the model's time period
            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id && t.GroupId == null &&
                            t.TransactionDate >= currentModel.StartDate &&
                            t.TransactionDate <= currentModel.EndDate &&
                            t.Status == TransactionStatus.APPROVED,
                include: query => query
                    .Include(t => t.Subcategory)
                    .ThenInclude(s => s.CategorySubcategories)
                    .ThenInclude(cs => cs.Category)
            );

            // Group transactions by category and calculate totals
            foreach (var spendingModelCategory in currentModel.SpendingModel.SpendingModelCategories)
            {
                var categoryTransactions = transactions.Where(t =>
                    t.Subcategory != null &&
                    t.Subcategory.CategorySubcategories.Any(cs =>
                        cs.CategoryId == spendingModelCategory.CategoryId));

                var categoryTotal = categoryTransactions.Sum(t => t.Amount);
                totalSpent += categoryTotal;

                // Calculate planned spending amount based on percentage of total income
                var planningSpent = (spendingModelCategory.PercentageAmount.Value / 100) * totalIncome;

                chartData.Add(new ChartSpendingCategoryModel
                {
                    CategoryName = spendingModelCategory.Category.Name,
                    TotalSpent = categoryTotal,
                    PlanningSpent = planningSpent,
                    PlannedPercentage = spendingModelCategory.PercentageAmount.Value,
                    ActualPercentage = 0, // Will be calculated after we have the total
                    OverSpent = categoryTotal > planningSpent ? categoryTotal - planningSpent : 0 // Calculate overspent amount
                });
            }

            // Calculate actual percentages
            foreach (var category in chartData)
            {
                // Calculate actual percentage based on percentage used of planned spending
                if (category.PlanningSpent > 0)
                {
                    // (TotalSpent / PlanningSpent) * PlannedPercentage = % used of planned percentage
                    category.ActualPercentage = Math.Round((category.TotalSpent / category.PlanningSpent) * category.PlannedPercentage, 2);
                }
                else
                {
                    category.ActualPercentage = 0;
                }
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new
                {
                    Categories = chartData,
                    TotalSpent = totalSpent,
                    TotalIncome = totalIncome,
                    TotalOverspent = chartData.Sum(c => c.OverSpent), // Add total overspent
                    StartDate = currentModel.StartDate,
                    EndDate = currentModel.EndDate
                }
            };
        }

        public async Task<BaseResultModel> GetChartSpendingModelAsync(Guid spendingModelId)
        {
            var userSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetByIdIncludeAsync(
                spendingModelId,
                include: query => query
                    .Include(usm => usm.SpendingModel)
                    .ThenInclude(sm => sm.SpendingModelCategories)
                    .ThenInclude(smc => smc.Category)
            );

            if (userSpendingModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "No spending model found"
                };
            }

            if (!userSpendingModel.SpendingModel.SpendingModelCategories.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SPENDING_MODEL_HAS_NO_CATEGORIES,
                    Message = "Current spending model has no categories"
                };
            }

            var chartData = new List<ChartSpendingCategoryModel>();
            decimal totalSpent = 0;

            // Get total income for the spending model period
            decimal totalIncome = await _unitOfWork.TransactionsRepository.GetTotalIncomeAsync(
                userId: userSpendingModel.UserId,
                groupId: null,
                startDate: userSpendingModel.StartDate.Value,
                endDate: userSpendingModel.EndDate.Value
            );

            // Get all transactions within the model's time period
            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == userSpendingModel.UserId && t.GroupId == null &&
                            t.TransactionDate >= userSpendingModel.StartDate &&
                            t.TransactionDate <= userSpendingModel.EndDate &&
                            t.Status == TransactionStatus.APPROVED,
                include: query => query
                    .Include(t => t.Subcategory)
                    .ThenInclude(s => s.CategorySubcategories)
                    .ThenInclude(cs => cs.Category)
            );

            // Group transactions by category and calculate totals
            foreach (var spendingModelCategory in userSpendingModel.SpendingModel.SpendingModelCategories)
            {
                var categoryTransactions = transactions.Where(t =>
                    t.Subcategory != null &&
                    t.Subcategory.CategorySubcategories.Any(cs =>
                        cs.CategoryId == spendingModelCategory.CategoryId));

                var categoryTotal = categoryTransactions.Sum(t => t.Amount);
                totalSpent += categoryTotal;

                // Calculate planned spending amount based on percentage of total income
                var planningSpent = (spendingModelCategory.PercentageAmount.Value / 100) * totalIncome;

                chartData.Add(new ChartSpendingCategoryModel
                {
                    CategoryName = spendingModelCategory.Category.Name,
                    TotalSpent = categoryTotal,
                    PlanningSpent = planningSpent,
                    PlannedPercentage = spendingModelCategory.PercentageAmount.Value,
                    ActualPercentage = 0, // Will be calculated after we have the total
                    OverSpent = categoryTotal > planningSpent ? categoryTotal - planningSpent : 0 // Calculate overspent amount
                });
            }

            // Calculate actual percentages
            foreach (var category in chartData)
            {
                // Calculate actual percentage based on percentage used of planned spending
                if (category.PlanningSpent > 0)
                {
                    // (TotalSpent / PlanningSpent) * PlannedPercentage = % used of planned percentage
                    category.ActualPercentage = Math.Round((category.TotalSpent / category.PlanningSpent) * category.PlannedPercentage, 2);
                }
                else
                {
                    category.ActualPercentage = 0;
                }
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new
                {
                    Categories = chartData,
                    TotalSpent = totalSpent,
                    TotalIncome = totalIncome,
                    TotalOverspent = chartData.Sum(c => c.OverSpent), // Add total overspent
                    StartDate = userSpendingModel.StartDate,
                    EndDate = userSpendingModel.EndDate
                }
            };
        }

        public async Task<BaseResultModel> GetTransactionsByUserSpendingModelAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter, Guid userSpendingModelId)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST,
                    Message = "User not found."
                };
            }

            var userSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetByIdAsync(userSpendingModelId);
            if (userSpendingModel == null || userSpendingModel.UserId != user.Id)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "Spending model not found for the user."
                };
            }

            transactionFilter.FromDate = userSpendingModel.StartDate;
            transactionFilter.ToDate = userSpendingModel.EndDate;

            var transactions = await _unitOfWork.TransactionsRepository.GetTransactionsFilterAsync(
                paginationParameter,
                transactionFilter,
                t => t.UserId == user.Id,
                include: query => query.Include(t => t.Subcategory)
            );

            var transactionModels = _mapper.Map<Pagination<TransactionModel>>(transactions);
            var result = PaginationHelper.GetPaginationResult(transactions, transactionModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_LIST_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> UpdateExpiredSpendingModelsAsync()
        {
            var currentTime = CommonUtils.GetCurrentTime();

            // Get all active spending models that have passed their end date
            var expiredModels = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.Status == UserSpendingModelStatus.ACTIVE
                    && usm.EndDate < currentTime
                    && !usm.IsDeleted
            );

            if (!expiredModels.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "No expired models found to update."
                };
            }

            var expiredModelUpdate = new List<UserSpendingModel>();

            // Update status to EXPIRED for all expired models
            foreach (var model in expiredModels)
            {
                model.Status = UserSpendingModelStatus.EXPIRED;
                model.UpdatedDate = currentTime;
                expiredModelUpdate.Add(model);
            }

            _unitOfWork.UserSpendingModelRepository.UpdateRangeAsync(expiredModelUpdate);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = $"Successfully updated {expiredModels.Count()} expired spending models.",
                Data = expiredModels.Count()
            };
        }

        public async Task<BaseResultModel> GetSubCategoriesCurrentSpendingModelAsync(CategoryCurrentSpendingModelFiter filter)
        {
            // Get current user
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);

            // Get current active spending model with related data
            var currentModels = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id 
                    && usm.EndDate > CommonUtils.GetCurrentTime()
                    && usm.Status == UserSpendingModelStatus.ACTIVE
                    && !usm.IsDeleted,
                include: query => query
                    .Include(usm => usm.SpendingModel)
                    .ThenInclude(sm => sm.SpendingModelCategories)
                    .ThenInclude(smc => smc.Category)
                    .ThenInclude(c => c.CategorySubcategories)
                    .ThenInclude(cs => cs.Subcategory)
            );

            var currentModel = currentModels.FirstOrDefault();
            if (currentModel == null)
            {
                throw new NotExistException("No active spending model found", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            var subcategories = currentModel.SpendingModel.SpendingModelCategories
                .Where(smc => smc.Category.Code != null && (string.IsNullOrEmpty(filter.Code) || smc.Category.Code == filter.Code)
                    && (string.IsNullOrEmpty(filter.Type) || smc.Category.Type.ToString() == filter.Type.ToString().ToUpper()))
                .SelectMany(smc => smc.Category.CategorySubcategories
                    .Select(cs => cs.Subcategory))
                .Where(s => s != null)
                .ToList();

            // If IsLastUsed is true, get and prioritize recently used subcategories
            if (filter.IsLastUsed.HasValue && filter.IsLastUsed.Value)
            {
                // Get recent transactions for this user
                var recentTransactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                    filter: t => t.UserId == user.Id 
                        && t.GroupId == null 
                        && t.Status == TransactionStatus.APPROVED
                        && t.SubcategoryId != null,
                    orderBy: query => query.OrderByDescending(t => t.TransactionDate)
                );

                // Get distinct subcategory IDs from recent transactions
                var recentSubcategoryIds = recentTransactions
                    .Select(t => t.SubcategoryId.Value)
                    .Distinct()
                    .Take(5)
                    .ToList();

                // Split subcategories into recent and others
                var recentSubcategories = subcategories
                    .Where(s => recentSubcategoryIds.Contains(s.Id))
                    .OrderBy(s => recentSubcategoryIds.IndexOf(s.Id));

                var otherSubcategories = subcategories
                    .Where(s => !recentSubcategoryIds.Contains(s.Id))
                    .OrderBy(s => s.Name);

                // Combine the lists with recent subcategories first
                subcategories = recentSubcategories.Concat(otherSubcategories).ToList();
            }
            else
            {
                // If not sorting by recent usage, sort by name
                subcategories = subcategories.OrderBy(s => s.Name).ToList();
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Subcategories retrieved successfully",
                Data = _mapper.Map<List<SubcategoryModel>>(subcategories)
            };
        }

        public async Task<BaseResultModel> GetCategoriesCurrentSpendingModelAsync()
        {
            // Get current user
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);

            // Get current active spending model with related data
            var currentModels = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id
                    && usm.EndDate > CommonUtils.GetCurrentTime()
                    && usm.Status == UserSpendingModelStatus.ACTIVE
                    && !usm.IsDeleted,
                include: query => query
                    .Include(usm => usm.SpendingModel)
                    .ThenInclude(sm => sm.SpendingModelCategories)
                    .ThenInclude(smc => smc.Category)
                    .ThenInclude(c => c.CategorySubcategories)
                    .ThenInclude(cs => cs.Subcategory)
            );

            var currentModel = currentModels.FirstOrDefault();
            if (currentModel == null)
            {
                throw new NotExistException("No active spending model found", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            var spendingModel = await _spendingModelService.GetSpendingModelByIdAsync(currentModel.SpendingModelId.Value);
            if (spendingModel == null)
            {
                throw new NotExistException("", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            var categories = currentModel.SpendingModel.SpendingModelCategories
                .Select(smc => smc.Category)
                .Where(s => s != null)
                .ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Categories retrieved successfully",
                Data = _mapper.Map<List<CategoryModel>>(categories)
            };
        }

        public async Task<BaseResultModel> GetSubCategoriesCurrentSpendingModelByUserIdAsync(Guid userId)
        {
            // Get secret key from header
            var secretKey = _httpContextAccessor.HttpContext?.Request.Headers["X-Webhook-Secret"].ToString();

            if (string.IsNullOrEmpty(secretKey) || secretKey != "thisIsSerectKeyPythonService")
            {
                throw new DefaultException("Invalid webhook secret key", MessageConstants.INVALID_WEBHOOK_SECRET);
            }

            var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Get current active spending model with related data
            var currentModels = await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id
                    && usm.EndDate > CommonUtils.GetCurrentTime()
                    && usm.Status == UserSpendingModelStatus.ACTIVE
                    && !usm.IsDeleted,
                include: query => query
                    .Include(usm => usm.SpendingModel)
                    .ThenInclude(sm => sm.SpendingModelCategories)
                    .ThenInclude(smc => smc.Category)
                    .ThenInclude(c => c.CategorySubcategories)
                    .ThenInclude(cs => cs.Subcategory)
            );

            var currentModel = currentModels.FirstOrDefault();
            if (currentModel == null)
            {
                throw new NotExistException("No active spending model found", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            var subcategories = currentModel.SpendingModel.SpendingModelCategories
                .SelectMany(smc => smc.Category.CategorySubcategories
                    .Select(cs => cs.Subcategory))
                .Where(s => s != null)
                .ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Subcategories retrieved successfully",
                Data = _mapper.Map<List<SubcategoryModel>>(subcategories)
            };
        }
    }
}
