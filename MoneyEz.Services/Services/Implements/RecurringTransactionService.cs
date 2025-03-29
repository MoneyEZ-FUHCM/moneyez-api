using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.RecurringTransactionModels;
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
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public RecurringTransactionService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> AddRecurringTransactionAsync(CreateRecurringTransactionModel model)
        {
            var user = await GetCurrentUserAsync();
            var subcategory = await GetSubcategoryAsync(model.SubcategoryId);
            var category = await GetCategoryBySubcategoryIdAsync(subcategory.Id);
            await ValidateSubcategoryInCurrentSpendingModelAsync(user.Id, model.SubcategoryId);

            var entity = _mapper.Map<RecurringTransaction>(model);
            entity.UserId = user.Id;
            entity.Type = category.Type ?? throw new DefaultException("Category is missing transaction type.", MessageConstants.CATEGORY_TYPE_INVALID);
            entity.Status = CommonsStatus.ACTIVE;
            entity.CreatedBy = user.Email;

            await _unitOfWork.RecurringTransactionRepository.AddAsync(entity);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.RECURRING_TRANSACTION_CREATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> GetAllRecurringTransactionsAsync(PaginationParameter paginationParameter, RecurringTransactionFilter filter)
        {
            var user = await GetCurrentUserAsync();

            var transactions = await _unitOfWork.RecurringTransactionRepository.ToPaginationIncludeAsync(
                paginationParameter,
                filter: t => t.UserId == user.Id
                    && (!filter.SubcategoryId.HasValue || t.SubcategoryId == filter.SubcategoryId)
                    && (!filter.FromDate.HasValue || t.StartDate >= filter.FromDate.Value)
                    && (!filter.ToDate.HasValue || t.StartDate <= filter.ToDate.Value)
                    && (!filter.IsActive.HasValue || (filter.IsActive.Value
                        ? t.Status == (int)CommonsStatus.ACTIVE
                        : t.Status != (int)CommonsStatus.ACTIVE)),
                include: q => q.Include(t => t.Subcategory)
            );

            var models = _mapper.Map<List<RecurringTransactionModel>>(transactions);
            var result = PaginationHelper.GetPaginationResult(transactions, models);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.RECURRING_TRANSACTION_LIST_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> GetRecurringTransactionByIdAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();

            var transaction = await _unitOfWork.RecurringTransactionRepository.GetByIdIncludeAsync(
                id,
                include: q => q.Include(t => t.Subcategory)
            );

            if (transaction == null || transaction.UserId != user.Id)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.RECURRING_TRANSACTION_ACCESS_DENIED,
                    Message = "Access denied: This recurring transaction does not belong to you."
                };
            }

            var model = _mapper.Map<RecurringTransactionModel>(transaction);
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.RECURRING_TRANSACTION_FETCHED_SUCCESS,
                Data = model
            };
        }

        public async Task<BaseResultModel> UpdateRecurringTransactionAsync(UpdateRecurringTransactionModel model)
        {
            var user = await GetCurrentUserAsync();
            var transaction = await GetTransactionByIdAsync(model.Id, user.Id);
            var subcategory = await GetSubcategoryAsync(model.SubcategoryId);
            var category = await GetCategoryBySubcategoryIdAsync(subcategory.Id);
            await ValidateSubcategoryInCurrentSpendingModelAsync(user.Id, model.SubcategoryId);

            _mapper.Map(model, transaction);
            transaction.UpdatedDate = CommonUtils.GetCurrentTime();
            transaction.UpdatedBy = user.Email;
            transaction.Type = category.Type ?? throw new DefaultException("Category is missing transaction type.", MessageConstants.CATEGORY_TYPE_INVALID);

            _unitOfWork.RecurringTransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.RECURRING_TRANSACTION_UPDATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> DeleteRecurringTransactionAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            var transaction = await GetTransactionByIdAsync(id, user.Id);

            _unitOfWork.RecurringTransactionRepository.SoftDeleteAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.RECURRING_TRANSACTION_DELETED_SUCCESS
            };
        }

        #region helper
        private async Task<User> GetCurrentUserAsync()
        {
            var email = _claimsService.GetCurrentUserEmail;
            return await _unitOfWork.UsersRepository.GetUserByEmailAsync(email)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);
        }

        private async Task<Subcategory> GetSubcategoryAsync(Guid subcategoryId)
        {
            return await _unitOfWork.SubcategoryRepository.GetByIdAsync(subcategoryId)
                ?? throw new NotExistException(MessageConstants.SUBCATEGORY_NOT_FOUND);
        }

        private async Task<Category> GetCategoryBySubcategoryIdAsync(Guid subcategoryId)
        {
            return await _unitOfWork.CategorySubcategoryRepository.GetCategoryBySubcategoryId(subcategoryId)
                ?? throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);
        }

        private async Task ValidateSubcategoryInCurrentSpendingModelAsync(Guid userId, Guid subcategoryId)
        {
            var currentSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(userId)
                ?? throw new DefaultException("Không tìm thấy UserSpendingModel đang hoạt động.", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);

            var allowedSubcategories = await _unitOfWork.CategorySubcategoryRepository.GetSubcategoriesBySpendingModelId(currentSpendingModel.SpendingModelId.Value);
            if (!allowedSubcategories.Any(s => s.Id == subcategoryId))
            {
                throw new DefaultException("Subcategory không nằm trong SpendingModel hiện tại.", MessageConstants.SUBCATEGORY_NOT_IN_SPENDING_MODEL);
            }
        }

        private async Task<RecurringTransaction> GetTransactionByIdAsync(Guid transactionId, Guid userId)
        {
            var transaction = await _unitOfWork.RecurringTransactionRepository.GetByIdAsync(transactionId)
                ?? throw new NotExistException(MessageConstants.RECURRING_TRANSACTION_NOT_FOUND);

            if (transaction.UserId != userId)
            {
                throw new DefaultException("You cannot modify another user's recurring transaction.",
                    MessageConstants.RECURRING_TRANSACTION_ACCESS_DENIED);
            }

            return transaction;
        }
        #endregion helper
    }
}
