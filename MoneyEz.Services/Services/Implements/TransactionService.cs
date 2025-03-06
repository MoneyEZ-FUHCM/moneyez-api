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

namespace MoneyEz.Services.Services.Implements
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;
        private readonly ITransactionNotificationService _transactionNotificationService;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService, ITransactionNotificationService transactionNotificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
            _transactionNotificationService = transactionNotificationService;
        }

        #region single user
        public async Task<BaseResultModel> GetAllTransactionsForUserAsync(PaginationParameter paginationParameter)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;

            if (string.IsNullOrEmpty(userEmail))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.TOKEN_NOT_VALID,
                    Message = "Unauthorized: Cannot retrieve user email."
                };
            }

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

            var transactions = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query.Include(t => t.Subcategory),
                filter: t => t.UserId == user.Id,
                orderBy: t => t.OrderByDescending(t => t.CreatedDate)
            );

            var transactionModels = _mapper.Map<List<TransactionModel>>(transactions);

            foreach (var transactionModel in transactionModels)
            {
                var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transactionModel.Id, "Transaction");
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
        public async Task<BaseResultModel> GetTransactionByIdAsync(Guid transactionId)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;

            if (string.IsNullOrEmpty(userEmail))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.TOKEN_NOT_VALID,
                    Message = "Unauthorized: Cannot retrieve user email."
                };
            }

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

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, "Transaction");
            transactionModel.Images = images.Select(i => i.ImageUrl).ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_FETCHED_SUCCESS,
                Data = transactionModel
            };
        }
        public async Task<BaseResultModel> CreateTransactionAsync(CreateTransactionModel model)
        {
            var user = await GetCurrentUserAsync();
            await ValidateSubcategoryInCurrentSpendingModel(model.SubcategoryId, user.Id);

            var transaction = _mapper.Map<Transaction>(model);
            transaction.UserId = user.Id;
            transaction.Status = TransactionStatus.APPROVED;
            transaction.CreatedBy = user.Email;

            await CheckAndNotifyCategorySpendingLimit(transaction, user);

            await _unitOfWork.TransactionsRepository.AddAsync(transaction);
            await UpdateFinancialGoalProgress(transaction, user);

            if (model.Images != null && model.Images.Any())
            {
                var images = model.Images.Select(url => new Image
                {
                    EntityId = transaction.Id,
                    EntityName = "Transaction",
                    ImageUrl = url
                }).ToList();

                await _unitOfWork.ImageRepository.AddRangeAsync(images);
            }

            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.TRANSACTION_CREATED_SUCCESS
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

            await CheckAndNotifyCategorySpendingLimit(transaction, user);

            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);

            await UpdateFinancialGoalProgress(transaction, user);

            var oldImages = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, "Transaction");
            _unitOfWork.ImageRepository.PermanentDeletedListAsync(oldImages);

            if (model.Images != null && model.Images.Any())
            {
                var images = model.Images.Select(url => new Image
                {
                    EntityId = transaction.Id,
                    EntityName = "Transaction",
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

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, "Transaction");
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
        public async Task<BaseResultModel> GetTransactionsByUserSpendingModelAsync(PaginationParameter paginationParameter, Guid userSpendingModelId)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            if (string.IsNullOrEmpty(userEmail))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.TOKEN_NOT_VALID,
                    Message = "Unauthorized: Cannot retrieve user email."
                };
            }

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

            var transactions = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query.Include(t => t.Subcategory),
                filter: t => t.UserId == user.Id
                             && t.TransactionDate >= userSpendingModel.StartDate
                             && t.TransactionDate <= userSpendingModel.EndDate,
                orderBy: t => t.OrderByDescending(t => t.CreatedDate)
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
            var category = await _unitOfWork.CategorySubcategoryRepository.GetCategoryBySubcategoryId(subcategory.Id);
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
                financialGoal.CurrentAmount = financialGoal.TargetAmount;
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
        public async Task<BaseResultModel> GetAllTransactionsForAdminAsync(PaginationParameter paginationParameter)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            if (string.IsNullOrEmpty(userEmail))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.TRANSACTION_ACCESS_DENIED,
                    Message = "Unauthorized: Cannot retrieve user email."
                };
            }

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

            var transactions = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query.Include(t => t.Subcategory).Include(t => t.User)
            );

            var result = _mapper.Map<Pagination<TransactionModel>>(transactions);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_LIST_FETCHED_SUCCESS,
                Data = new ModelPaging
                {
                    Data = result,
                    MetaData = new
                    {
                        result.TotalCount,
                        result.PageSize,
                        result.CurrentPage,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    }
                }
            };
        }

        //group
        public async Task<BaseResultModel> GetTransactionByGroupIdAsync(PaginationParameter paginationParameter, TransactionFilter transactionFilter)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            if (!transactionFilter.GroupId.HasValue)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                transactionFilter.GroupId.Value,
                include: q => q.Include(g => g.GroupMembers)
                             .Include(g => g.Transactions)
            );

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Verify user is a member of the group
            var isMember = groupFund.GroupMembers.Any(member =>
                member.UserId == user.Id &&
                member.Status == GroupMemberStatus.ACTIVE);

            if (!isMember)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            var transactions = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query
                    .Include(t => t.Subcategory)
                    .Include(t => t.User),
                filter: t =>
                    (!transactionFilter.GroupId.HasValue || t.GroupId == transactionFilter.GroupId.Value) &&
                    (!transactionFilter.UserId.HasValue || t.UserId == transactionFilter.UserId.Value),
                orderBy: t => t.OrderByDescending(t => t.TransactionDate)
            );

            var transactionModels = _mapper.Map<List<TransactionModel>>(transactions);
            var result = PaginationHelper.GetPaginationResult(transactions, transactionModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_FETCHED_SUCCESS,
                Data = result
            };
        }
    }
}
