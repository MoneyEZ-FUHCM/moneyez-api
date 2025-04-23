using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.WebhookModels;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.TransactionModels.Group;
using MoneyEz.Services.BusinessModels.GroupFund;
using MoneyEz.Services.BusinessModels.TransactionModels.Reports;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Query;

namespace MoneyEz.Services.Services.Implements
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;
        private readonly ITransactionNotificationService _transactionNotificationService;
        private readonly IGroupTransactionService _groupTransactionService;

        public TransactionService(IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IClaimsService claimsService, 
            ITransactionNotificationService transactionNotificationService,
            IGroupTransactionService groupTransactionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
            _transactionNotificationService = transactionNotificationService;
            _groupTransactionService = groupTransactionService;
        }

        #region single user
        public async Task<BaseResultModel> GetAllTransactionsForUserAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter)
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

            var transactions = await _unitOfWork.TransactionsRepository.GetTransactionsFilterAsync(
                paginationParameter,
                transactionFilter,
                condition: t => t.UserId == user.Id,
                include: query => query.Include(t => t.Subcategory)
            );

            var transactionModels = _mapper.Map<List<TransactionModel>>(transactions);

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityNameAsync(EntityName.TRANSACTION.ToString());
            foreach (var transactionModel in transactionModels)
            {
                var transactionImages = images.Where(i => i.EntityId == transactionModel.Id).ToList();
                transactionModel.Images = transactionImages.Select(i => i.ImageUrl).ToList();
            }

            var result = PaginationHelper.GetPaginationResult(transactions, transactionModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_LIST_FETCHED_SUCCESS,
                Data = result
            };
        }
        public async Task<BaseResultModel> GetTransactionByIdAsync(Guid transactionId)
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

            var transaction = await _unitOfWork.TransactionsRepository.GetByIdIncludeAsync(
                transactionId,
                include: query => query.Include(t => t.Subcategory)
            );

            if (transaction == null || transaction.UserId != user.Id)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.TRANSACTION_ACCESS_DENIED,
                    Message = "Access denied: You can only view your own transactions."
                };
            }

            var transactionModel = _mapper.Map<TransactionModel>(transaction);

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, EntityName.TRANSACTION.ToString());
            transactionModel.Images = images.Select(i => i.ImageUrl).ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_FETCHED_SUCCESS,
                Data = transactionModel
            };
        }
        public async Task<BaseResultModel> CreateTransactionAsync(CreateTransactionModel model, string email)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email)
                ?? throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            await ValidateSubcategoryInCurrentSpendingModel(model.SubcategoryId, user.Id);

            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId)
                ?? throw new NotExistException(MessageConstants.SUBCATEGORY_NOT_FOUND);

            var category = await _unitOfWork.CategorySubcategoryRepository.GetCategoryBySubcategoryId(subcategory.Id)
                ?? throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);

            var currentSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(user.Id)
                ?? throw new DefaultException("Không tìm thấy UserSpendingModel đang hoạt động.", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);

            var transaction = _mapper.Map<Transaction>(model);
            transaction.UserId = user.Id;
            transaction.Status = TransactionStatus.APPROVED;
            transaction.Type = category.Type ?? throw new DefaultException("Danh mục không có TransactionType hợp lệ.", MessageConstants.CATEGORY_TYPE_INVALID);
            transaction.UserSpendingModelId = currentSpendingModel.Id;
            transaction.CreatedBy = user.Email;

            await CheckAndNotifyCategorySpendingLimit(transaction, user);

            await _unitOfWork.TransactionsRepository.AddAsync(transaction);
            await UpdateFinancialGoalProgress(transaction, user);

            if (model.Images != null && model.Images.Any())
            {
                var images = model.Images.Select(url => new Image
                {
                    EntityId = transaction.Id,
                    EntityName = EntityName.TRANSACTION.ToString(),
                    ImageUrl = url
                }).ToList();

                await _unitOfWork.ImageRepository.AddRangeAsync(images);
            }

            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.TRANSACTION_CREATED_SUCCESS,
                Data = _mapper.Map<TransactionModel>(transaction)
            };
        }
        public async Task<BaseResultModel> UpdateTransactionAsync(UpdateTransactionModel model)
        {
            var user = await GetCurrentUserAsync();

            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.Id)
                ?? throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            if (transaction.UserId != user.Id)
            {
                throw new DefaultException("You can only modify your own transactions.", MessageConstants.TRANSACTION_UPDATE_DENIED);
            }

            await UpdateFinancialGoalProgress(transaction, user, isRollback: true);

            _mapper.Map(model, transaction);
            transaction.UpdatedBy = user.Email;

            await ValidateSubcategoryInCurrentSpendingModel(transaction.SubcategoryId.Value, user.Id);

            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(transaction.SubcategoryId.Value)
                ?? throw new NotExistException(MessageConstants.SUBCATEGORY_NOT_FOUND);

            var category = await _unitOfWork.CategorySubcategoryRepository.GetCategoryBySubcategoryId(subcategory.Id)
                ?? throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);

            var currentSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(user.Id)
                ?? throw new DefaultException("Không tìm thấy UserSpendingModel đang hoạt động.", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);

            transaction.Type = category.Type ?? throw new DefaultException("Danh mục không có TransactionType hợp lệ.", MessageConstants.CATEGORY_TYPE_INVALID);
            transaction.UserSpendingModelId = currentSpendingModel.Id;

            await CheckAndNotifyCategorySpendingLimit(transaction, user);

            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);

            await UpdateFinancialGoalProgress(transaction, user);

            var oldImages = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, EntityName.TRANSACTION.ToString());
            _unitOfWork.ImageRepository.PermanentDeletedListAsync(oldImages);

            if (model.Images != null && model.Images.Any())
            {
                var images = model.Images.Select(url => new Image
                {
                    EntityId = transaction.Id,
                    EntityName = EntityName.TRANSACTION.ToString(),
                    ImageUrl = url
                }).ToList();

                await _unitOfWork.ImageRepository.AddRangeAsync(images);
            }

            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_UPDATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> DeleteTransactionAsync(Guid transactionId)
        {
            var user = await GetCurrentUserAsync();

            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(transactionId)
                ?? throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            if (transaction.UserId != user.Id)
            {
                throw new DefaultException("You can only delete your own transactions.", MessageConstants.TRANSACTION_DELETE_DENIED);
            }

            await UpdateFinancialGoalProgress(transaction, user, isRollback: true);

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, EntityName.TRANSACTION.ToString());
            if (images.Any())
            {
                _unitOfWork.ImageRepository.PermanentDeletedListAsync(images);
            }

            _unitOfWork.TransactionsRepository.PermanentDeletedAsync(transaction);

            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_DELETED_SUCCESS
            };
        }
        public async Task<BaseResultModel> GetTransactionsByUserSpendingModelAsync(PaginationParameter paginationParameter,
                                                                                    Guid userSpendingModelId,
                                                                                    TransactionFilter transactionFilter)
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
                condition: t => t.Status == TransactionStatus.APPROVED && t.UserId == user.Id,
                include: query => query.Include(t => t.Subcategory)
            );

            var transactionModels = _mapper.Map<Pagination<TransactionModel>>(transactions);

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityNameAsync(EntityName.TRANSACTION.ToString());
            foreach (var transactionModel in transactionModels)
            {
                var transactionImage = images.Where(i => i.EntityId == transactionModel.Id).ToList();
                transactionModel.Images = images.Select(i => i.ImageUrl).ToList();
            }

            var result = PaginationHelper.GetPaginationResult(transactions, transactionModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_LIST_FETCHED_SUCCESS,
                Data = result
            };
        }


        private async Task<User> GetCurrentUserAsync()
        {
            var email = _claimsService.GetCurrentUserEmail;
            return await _unitOfWork.UsersRepository.GetUserByEmailAsync(email)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);
        }

        private async Task ValidateSubcategoryInCurrentSpendingModel(Guid subcategoryId, Guid userId)
        {
            var currentSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(userId)
                ?? throw new DefaultException("User has no active spending model.", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);

            var allowedSubcategories = await _unitOfWork.CategorySubcategoryRepository.GetSubcategoriesBySpendingModelId(currentSpendingModel.SpendingModelId.Value);
            if (!allowedSubcategories.Any(s => s.Id == subcategoryId))
            {
                throw new DefaultException("Selected subcategory is not allowed in the current spending model.", MessageConstants.SUBCATEGORY_NOT_IN_SPENDING_MODEL);
            }
        }

        private async Task CheckAndNotifyCategorySpendingLimit(Transaction transaction, User user)
        {
            var currentSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(transaction.UserId.Value);
            var totalIncome = await _unitOfWork.TransactionsRepository.GetTotalIncomeAsync(
                transaction.UserId.Value, null, currentSpendingModel.StartDate.Value, currentSpendingModel.EndDate.Value);

            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(transaction.SubcategoryId.Value);
            var category = await _unitOfWork.CategorySubcategoryRepository
                .GetCategoryInCurrentSpendingModel(subcategory.Id, currentSpendingModel.SpendingModelId.Value)
                ?? throw new DefaultException(
                    "Subcategory này không thuộc danh mục nào trong mô hình chi tiêu hiện tại.",
                    MessageConstants.SUBCATEGORY_NOT_IN_SPENDING_MODEL
                );
            var spendingModelCategory = await _unitOfWork.SpendingModelCategoryRepository.GetByModelAndCategory(
                currentSpendingModel.SpendingModelId.Value, category.Id);

            decimal categoryBudget = totalIncome * (spendingModelCategory.PercentageAmount ?? 0) / 100m;

            decimal totalCategoryTransaction = transaction.Type == TransactionType.EXPENSE
                ? await _unitOfWork.TransactionsRepository.GetTotalExpenseByCategory(transaction.UserId.Value, category.Id, currentSpendingModel.StartDate.Value, currentSpendingModel.EndDate.Value)
                : await _unitOfWork.TransactionsRepository.GetTotalIncomeByCategory(transaction.UserId.Value, category.Id, currentSpendingModel.StartDate.Value, currentSpendingModel.EndDate.Value);

            if ((totalCategoryTransaction + transaction.Amount) > categoryBudget)
            {
                decimal exceededAmount = (totalCategoryTransaction + transaction.Amount) - categoryBudget;
                await _transactionNotificationService.NotifyBudgetExceededAsync(user, category, exceededAmount, transaction.Type);
            }
        }


        private async Task UpdateFinancialGoalProgress(Transaction transaction, User user, bool isRollback = false)
        {
            var financialGoal = await _unitOfWork.FinancialGoalRepository.GetActiveGoalByUserAndSubcategory(transaction.UserId.Value, transaction.SubcategoryId.Value);

            if (financialGoal == null || financialGoal.Status == FinancialGoalStatus.COMPLETED)
            {
                return;
            }

            var adjustmentAmount = isRollback ? -transaction.Amount : transaction.Amount;

            financialGoal.CurrentAmount += adjustmentAmount;

            if (financialGoal.CurrentAmount >= financialGoal.TargetAmount)
            {
                //financialGoal.CurrentAmount = financialGoal.TargetAmount;
                financialGoal.Status = FinancialGoalStatus.COMPLETED;
                financialGoal.ApprovalStatus = ApprovalStatus.APPROVED;

                await _transactionNotificationService.NotifyGoalAchievedAsync(user, financialGoal);
            }
            else if (!isRollback)
            {
                await _transactionNotificationService.NotifyGoalProgressTrackingAsync(user, financialGoal);
            }

            _unitOfWork.FinancialGoalRepository.UpdateAsync(financialGoal);
        }

        #endregion single user

        //admin
        public async Task<BaseResultModel> GetAllTransactionsForAdminAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter)
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

            if (user.Role != RolesEnum.ADMIN)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.TRANSACTION_ADMIN_ACCESS_DENIED,
                    Message = "Access denied: Only Admins can view all transactions."
                };
            }

            var transactions = await _unitOfWork.TransactionsRepository.GetTransactionsFilterAsync(
                paginationParameter,
                transactionFilter,
                include: query => query.Include(t => t.Subcategory).Include(t => t.User)
            );

            var transactionModels = _mapper.Map<Pagination<TransactionModel>>(transactions);

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityNameAsync(EntityName.TRANSACTION.ToString());
            foreach (var transactionModel in transactionModels)
            {
                var transactionImages = images.Where(i => i.EntityId == transactionModel.Id).ToList();
                transactionModel.Images = transactionImages.Select(i => i.ImageUrl).ToList();
            }

            var result = PaginationHelper.GetPaginationResult(transactions, transactionModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_LIST_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> CategorizeTransactionAsync(CategorizeTransactionModel model)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);

            // validate
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.TransactionId);
            if (transaction == null)
            {
                throw new NotExistException("", MessageConstants.TRANSACTION_NOT_FOUND);
            }

            if (transaction.UserId != user.Id)
            {
                throw new DefaultException("You can only categorize your own transactions.", MessageConstants.TRANSACTION_UPDATE_DENIED);
            }

            if (model.CategorizeTransaction == CategorizeTransaction.PERSONAL)
            {
                // for personal

                var updatePersonalTransaction = new UpdateTransactionModel
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    SubcategoryId = model.SubcategoryId != null ? model.SubcategoryId.Value : transaction.SubcategoryId.Value,
                };

                return await UpdateTransactionAsync(updatePersonalTransaction);
            }
            else
            {
                if (model.GroupId == null)
                {
                    throw new DefaultException("Transaction is not in any group.", MessageConstants.TRANSACTION_NOT_IN_GROUP);
                }

                // for group
                var updateGroupTransaction = new UpdateGroupTransactionModel
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    GroupId = model.GroupId.Value,
                    Type = transaction.Type,
                };

                return await _groupTransactionService.UpdateGroupTransactionAsync(updateGroupTransaction);
            }
        }

        #region report

        public async Task<BaseResultModel> GetYearReportAsync(int year, string type)
        {
            var userEmail = _claimsService.GetCurrentUserEmail;
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

            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id &&
                             t.TransactionDate!.Value.Year == year &&
                             t.SubcategoryId != null,
                include: IncludeFullCategoryNavigation()
            );

            var reportType = ConvertReportTransactionType(type);

            var monthly = new List<MonthAmountModel>();

            for (int month = 1; month <= 12; month++)
            {
                var monthTransactions = transactions
                    .Where(t => t.TransactionDate!.Value.Month == month)
                    .AsQueryable();

                var value = reportType == ReportTransactionType.TOTAL
                    ? GetIncomeExpenseTotal(monthTransactions).total
                    : GetTotalByType(monthTransactions, reportType);

                monthly.Add(new MonthAmountModel
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                    Amount = value
                });
            }

            decimal total = reportType == ReportTransactionType.TOTAL
                ? GetIncomeExpenseTotal(transactions.AsQueryable()).total
                : GetTotalByType(transactions.AsQueryable(), reportType);

            var currentMonth = DateTime.Now.Month;
            var monthsElapsed = (year == DateTime.Now.Year) ? currentMonth : 12;
            var avg = monthsElapsed > 0 ? total / monthsElapsed : 0;

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new YearTransactionReportModel
                {
                    Year = year,
                    Type = reportType.ToString(),
                    Total = total,
                    Average = avg,
                    MonthlyData = monthly
                }
            };
        }

        public async Task<BaseResultModel> GetCategoryYearReportAsync(int year, string type)
        {
            var userEmail = _claimsService.GetCurrentUserEmail;
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

            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id &&
                             t.TransactionDate!.Value.Year == year &&
                             t.SubcategoryId != null,
                include: IncludeFullCategoryNavigation()
            );

            var reportType = ConvertReportTransactionType(type);

            var filtered = _unitOfWork.TransactionsRepository
                .FilterByType(transactions.AsQueryable(), reportType);

            decimal total = reportType == ReportTransactionType.TOTAL
                ? GetIncomeExpenseTotal(transactions.AsQueryable()).total
                : filtered.Sum(t => t.Amount);

            var categories = filtered
                .GroupBy(t => t.Subcategory!)
                .Select(g => new CategoryAmountModel
                {
                    Name = g.Key.Name,
                    Icon = g.Key.Icon,
                    Amount = g.Sum(t => t.Amount),
                    Percentage = total == 0 ? 0 : Math.Round((double)(g.Sum(t => t.Amount) / total * 100), 2)
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new CategoryYearTransactionReportModel
                {
                    Year = year,
                    Type = reportType.ToString(),
                    Total = total,
                    Categories = categories
                }
            };
        }

        public async Task<BaseResultModel> GetAllTimeReportAsync()
        {
            var userEmail = _claimsService.GetCurrentUserEmail;
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

            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id && t.SubcategoryId != null,
                include: IncludeFullCategoryNavigation()
            );

            var (income, expense, total) = GetIncomeExpenseTotal(transactions.AsQueryable());

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new AllTimeTransactionSummaryModel
                {
                    Income = income,
                    Expense = expense,
                    Total = total
                }
            };
        }

        public async Task<BaseResultModel> GetAllTimeCategoryReportAsync(string type)
        {
            var userEmail = _claimsService.GetCurrentUserEmail;
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

            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id && t.SubcategoryId != null,
                include: IncludeFullCategoryNavigation()
            );

            var reportType = ConvertReportTransactionType(type);

            var filtered = _unitOfWork.TransactionsRepository
                .FilterByType(transactions.AsQueryable(), reportType);

            decimal total = reportType == ReportTransactionType.TOTAL
                ? GetIncomeExpenseTotal(transactions.AsQueryable()).total
                : filtered.Sum(t => t.Amount);

            var categories = filtered
                .GroupBy(t => t.Subcategory!)
                .Select(g => new CategoryAmountModel
                {
                    Name = g.Key.Name,
                    Icon = g.Key.Icon,
                    Amount = g.Sum(t => t.Amount),
                    Percentage = total == 0 ? 0 : Math.Round((double)(g.Sum(t => t.Amount) / total * 100), 2)
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new AllTimeCategoryTransactionReportModel
                {
                    Type = reportType.ToString(),
                    Total = total,
                    Categories = categories
                }
            };
        }

        public async Task<BaseResultModel> GetBalanceYearReportAsync(int year)
        {
            var userEmail = _claimsService.GetCurrentUserEmail;
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

            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id &&
                             t.TransactionDate!.Value.Year == year &&
                             t.SubcategoryId != null,
                include: IncludeFullCategoryNavigation()
            );

            var monthlyBalances = new List<MonthlyBalanceModel>();
            decimal currentBalance = 0;

            for (int month = 1; month <= 12; month++)
            {
                var monthTransactions = transactions
                    .Where(t => t.TransactionDate!.Value.Month == month)
                    .AsQueryable();

                var (income, expense, delta) = GetIncomeExpenseTotal(monthTransactions);
                currentBalance += delta;

                monthlyBalances.Add(new MonthlyBalanceModel
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                    Balance = currentBalance
                });
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new BalanceYearTransactionReportModel
                {
                    Year = year,
                    Balances = monthlyBalances
                }
            };
        }

     /*   public async Task<BaseResultModel> GetAllTimeCategoryReportAsyncV2(string type)
        {
            var userEmail = _claimsService.GetCurrentUserEmail;
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

            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id,
                include: q => q.Include(t => t.Subcategory)
                    .ThenInclude(c => c.CategorySubcategories)
                    .ThenInclude(cs => cs.Category)
            );

            // parse type to enum
            ReportTransactionType reportTransactionType;
            switch (type.ToLower())
            {
                case "expense":
                    reportTransactionType = ReportTransactionType.Expense;
                    break;
                case "income":
                    reportTransactionType = ReportTransactionType.Income;
                    break;
                case "total":
                    reportTransactionType = ReportTransactionType.Total;
                    break;
                default:
                    reportTransactionType = ReportTransactionType.Total;
                    break;
            }

            // Filter transactions and join with category information
            var transactionsWithCategory = transactions
                .Where(t => t.Subcategory != null)
                .SelectMany(t => t.Subcategory.CategorySubcategories
                    .Select(cs => new {
                        Transaction = t,
                        CategoryType = cs.Category.Type
                    }))
                .Where(x => reportTransactionType == ReportTransactionType.Total ||
                           (reportTransactionType == ReportTransactionType.Expense && x.CategoryType == TransactionType.EXPENSE) ||
                           (reportTransactionType == ReportTransactionType.Income && x.CategoryType == TransactionType.INCOME))
                .ToList();

            var total = transactionsWithCategory.Sum(t => t.Transaction.Amount);

            var categories = transactionsWithCategory
                .GroupBy(x => new {
                    SubcategoryId = x.Transaction.SubcategoryId,
                    SubcategoryName = x.Transaction.Subcategory.Name,
                    SubcategoryIcon = x.Transaction.Subcategory.Icon,
                    CategoryType = x.CategoryType
                })
                .Select(g => new CategoryAmountModel
                {
                    Name = g.Key.SubcategoryName,
                    Icon = g.Key.SubcategoryIcon,
                    Amount = g.Sum(x => x.Transaction.Amount),
                    Percentage = total == 0 ? 0 : Math.Round((double)(g.Sum(x => x.Transaction.Amount) / total * 100), 2),
                    CategoryType = g.Key.CategoryType.ToString()
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new AllTimeCategoryTransactionReportModel
                {
                    Type = reportTransactionType.ToString().ToUpper(),
                    Total = total,
                    Categories = categories
                }
            };
        }

        public async Task<BaseResultModel> GetCategoryYearReportAsyncV2(int year, string type)
        {
            var userEmail = _claimsService.GetCurrentUserEmail;
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

            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == user.Id &&
                             t.TransactionDate!.Value.Year == year,
                include: q => q.Include(t => t.Subcategory)
                    .ThenInclude(c => c.CategorySubcategories)
                    .ThenInclude(cs => cs.Category)
            );

            // parse type to enum
            ReportTransactionType reportTransactionType;
            switch (type.ToLower())
            {
                case "expense":
                    reportTransactionType = ReportTransactionType.Expense;
                    break;
                case "income":
                    reportTransactionType = ReportTransactionType.Income;
                    break;
                case "total":
                    reportTransactionType = ReportTransactionType.Total;
                    break;
                default:
                    reportTransactionType = ReportTransactionType.Total;
                    break;
            }

            // Filter transactions and join with category information
            var transactionsWithCategory = transactions
                .Where(t => t.Subcategory != null)
                .SelectMany(t => t.Subcategory.CategorySubcategories
                    .Select(cs => new {
                        Transaction = t,
                        CategoryType = cs.Category.Type
                    }))
                .Where(x => reportTransactionType == ReportTransactionType.Total ||
                           (reportTransactionType == ReportTransactionType.Expense && x.CategoryType == TransactionType.EXPENSE) ||
                           (reportTransactionType == ReportTransactionType.Income && x.CategoryType == TransactionType.INCOME))
                .ToList();

            var total = transactionsWithCategory.Sum(t => t.Transaction.Amount);

            var categories = transactionsWithCategory
                .GroupBy(x => new {
                    SubcategoryId = x.Transaction.SubcategoryId,
                    SubcategoryName = x.Transaction.Subcategory.Name,
                    SubcategoryIcon = x.Transaction.Subcategory.Icon,
                    CategoryType = x.CategoryType
                })
                .Select(g => new CategoryAmountModel
                {
                    Name = g.Key.SubcategoryName,
                    Icon = g.Key.SubcategoryIcon,
                    Amount = g.Sum(x => x.Transaction.Amount),
                    Percentage = total == 0 ? 0 : Math.Round((double)(g.Sum(x => x.Transaction.Amount) / total * 100), 2),
                    CategoryType = g.Key.CategoryType.ToString()
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new CategoryYearTransactionReportModel
                {
                    Year = year,
                    Type = reportTransactionType.ToString().ToUpper(),
                    Total = total,
                    Categories = categories
                }
            };
        }*/

        #endregion report

        #region helper
        private (decimal income, decimal expense, decimal total) GetIncomeExpenseTotal(IQueryable<Transaction> source)
        {
            var income = _unitOfWork.TransactionsRepository
                .FilterByType(source, ReportTransactionType.INCOME)
                .Sum(t => t.Amount);

            var expense = _unitOfWork.TransactionsRepository
                .FilterByType(source, ReportTransactionType.EXPENSE)
                .Sum(t => t.Amount);

            return (income, expense, income - expense);
        }

        private decimal GetTotalByType(IQueryable<Transaction> source, ReportTransactionType type)
        {
            return _unitOfWork.TransactionsRepository
                .FilterByType(source, type)
                .Sum(t => t.Amount);
        }

        private Func<IQueryable<Transaction>, IIncludableQueryable<Transaction, object>> IncludeFullCategoryNavigation()
        {
            return q => q
                .Include(t => t.Subcategory!)
                .ThenInclude(sc => sc.CategorySubcategories)
                .ThenInclude(cs => cs.Category);
        }

        private ReportTransactionType ConvertReportTransactionType(string type)
        {
            return type.ToLower() switch
            {
                "income" => ReportTransactionType.INCOME,
                "expense" => ReportTransactionType.EXPENSE,
                "total" => ReportTransactionType.TOTAL,
                _ => ReportTransactionType.TOTAL
            };
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

        /// <summary>
        /// Calculates the recommended withdrawal limit for a member based on their contribution percentage
        /// </summary>
        /// <param name="groupFund">The group fund</param>
        /// <param name="userId">The user ID of the member</param>
        /// <returns>A tuple containing (maxWithdrawalLimit, contributionPercentage)</returns>
        private async Task<(decimal maxWithdrawalLimit, decimal contributionPercentage)> CalculateWithdrawalLimitAsync(
            GroupFund groupFund, Guid userId)
        {
            // Calculate total deposits by the group member
            var memberTotalDeposits = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.GroupId == groupFund.Id &&
                            t.UserId == userId &&
                            t.Type == TransactionType.INCOME &&
                            t.Status == TransactionStatus.APPROVED
            );

            decimal totalDepositAmount = memberTotalDeposits.Sum(t => t.Amount);

            // Calculate current balance
            var memberBalance = await CalculateMemberBalanceAsync(groupFund.Id, userId);

            // Get all approved deposits for the group to calculate total contributions
            var allGroupDeposits = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.GroupId == groupFund.Id &&
                            t.Type == TransactionType.INCOME &&
                            t.Status == TransactionStatus.APPROVED
            );

            decimal totalGroupDepositAmount = allGroupDeposits.Sum(t => t.Amount);

            // Calculate contribution percentage
            decimal contributionPercentage = totalGroupDepositAmount > 0
                ? totalDepositAmount / totalGroupDepositAmount * 100
                : 0;

            // Calculate withdrawal limit based on contribution percentage
            decimal recommendedWithdrawalLimit = Math.Round(groupFund.CurrentBalance * (contributionPercentage / 100), 0);

            // Determine final maximum withdrawal limit
            // It should not exceed member's current balance or their total deposits
            decimal maxWithdrawalLimit = Math.Min(
                Math.Min(totalDepositAmount, memberBalance),
                recommendedWithdrawalLimit
            );

            return (maxWithdrawalLimit, contributionPercentage);
        }

        #endregion helper
    }
}
