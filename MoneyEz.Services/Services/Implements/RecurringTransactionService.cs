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

            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId)
                ?? throw new NotExistException(MessageConstants.SUBCATEGORY_NOT_FOUND);

            var category = await _unitOfWork.CategorySubcategoryRepository.GetCategoryBySubcategoryId(subcategory.Id)
                ?? throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);

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
                    && (!filter.FromDate.HasValue || t.StartDate >= filter.FromDate)
                    && (!filter.ToDate.HasValue || t.StartDate <= filter.ToDate)
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

            var transaction = await _unitOfWork.RecurringTransactionRepository.GetByIdAsync(model.Id)
                ?? throw new NotExistException(MessageConstants.RECURRING_TRANSACTION_NOT_FOUND);

            if (transaction.UserId != user.Id)
            {
                throw new DefaultException("You cannot update another user's recurring transaction.",
                    MessageConstants.RECURRING_TRANSACTION_ACCESS_DENIED);
            }

            _mapper.Map(model, transaction);
            transaction.UpdatedDate = CommonUtils.GetCurrentTime();
            transaction.UpdatedBy = user.Email;

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

            var transaction = await _unitOfWork.RecurringTransactionRepository.GetByIdAsync(id)
                ?? throw new NotExistException(MessageConstants.RECURRING_TRANSACTION_NOT_FOUND);

            if (transaction.UserId != user.Id)
            {
                throw new DefaultException("You cannot delete another user's recurring transaction.",
                    MessageConstants.RECURRING_TRANSACTION_DELETE_DENIED);
            }

            _unitOfWork.RecurringTransactionRepository.SoftDeleteAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.RECURRING_TRANSACTION_DELETED_SUCCESS
            };
        }

        private async Task<User> GetCurrentUserAsync()
        {
            var email = _claimsService.GetCurrentUserEmail;
            return await _unitOfWork.UsersRepository.GetUserByEmailAsync(email)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);
        }
    }
}
