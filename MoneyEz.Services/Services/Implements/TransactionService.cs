using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Repositories.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ** User Transactions **
        public async Task<BaseResultModel> GetUserTransactionsAsync(PaginationParameter paginationParameter)
        {
            var transactions = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query.Include(t => t.Subcategory),
                filter: t => t.GroupId == null
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
        public async Task<BaseResultModel> GetUserTransactionByIdAsync(Guid id)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdIncludeAsync(
                id,
                include: query => query.Include(t => t.Subcategory)
            );

            if (transaction == null)
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_FETCHED_SUCCESS,
                Data = _mapper.Map<TransactionModel>(transaction)
            };
        }
        public async Task<BaseResultModel> CreateTransactionForUserAsync(CreateTransactionModel model)
        {
            var transaction = _mapper.Map<Transaction>(model);
            transaction.ApprovalRequired = false;
            transaction.Status = TransactionStatus.APPROVED; // Cá nhân không cần duyệt

            await _unitOfWork.TransactionsRepository.AddAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.TRANSACTION_CREATED_SUCCESS
            };
        }
        public async Task<BaseResultModel> UpdateTransactionForUserAsync(UpdateTransactionModel model)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.Id);
            if (transaction == null)
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            _mapper.Map(model, transaction);
            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_UPDATED_SUCCESS
            };
        }
        public async Task<BaseResultModel> RemoveUserTransactionAsync(Guid id)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(id);
            if (transaction == null)
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            _unitOfWork.TransactionsRepository.PermanentDeletedAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_DELETED_SUCCESS
            };
        }

        // ** Group Transactions **
        public async Task<BaseResultModel> GetGroupTransactionsAsync(PaginationParameter paginationParameter)
        {
            var transactions = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query.Include(t => t.User),
                filter: t => t.GroupId != null
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
        public async Task<BaseResultModel> GetGroupTransactionByUserIdAsync(Guid userId)
        {
            var transactions = await _unitOfWork.TransactionsRepository.GetAllAsync();

            var userTransactions = transactions
                .Where(t => t.UserId == userId && t.GroupId != null)
                .ToList();

            if (!userTransactions.Any())
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            var result = _mapper.Map<List<TransactionModel>>(userTransactions);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_FETCHED_SUCCESS,
                Data = result
            };
        }
        public async Task<BaseResultModel> CreateTransactionForGroupAsync(CreateTransactionModel model)
        {
            var userRole = await _unitOfWork.GroupMemberRepository.GetUserRoleInGroup(model.UserId.Value, model.GroupId.Value);

            if (userRole == null)
                throw new DefaultException(MessageConstants.USER_NOT_IN_GROUP);

            var transaction = _mapper.Map<Transaction>(model);
            transaction.ApprovalRequired = userRole != RoleGroup.LEADER;
            transaction.Status = userRole == RoleGroup.LEADER ? TransactionStatus.APPROVED : TransactionStatus.PENDING;

            await _unitOfWork.TransactionsRepository.AddAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.TRANSACTION_CREATED_SUCCESS
            };
        }
        public async Task<BaseResultModel> UpdateTransactionForGroupAsync(UpdateTransactionModel model)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.Id);
            if (transaction == null)
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            if (transaction.Status == TransactionStatus.APPROVED)
                throw new DefaultException(MessageConstants.TRANSACTION_CANNOT_UPDATE_APPROVED);

            _mapper.Map(model, transaction);
            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_UPDATED_SUCCESS
            };
        }
        public async Task<BaseResultModel> RemoveGroupTransactionAsync(Guid id)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(id);
            if (transaction == null)
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            if (transaction.Status == TransactionStatus.APPROVED)
                throw new DefaultException(MessageConstants.TRANSACTION_CANNOT_DELETE_APPROVED);

            _unitOfWork.TransactionsRepository.PermanentDeletedAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_DELETED_SUCCESS
            };
        }

        // ** Approve/Reject Transaction **
        public async Task<BaseResultModel> ApproveTransactionAsync(Guid id)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(id);
            if (transaction == null)
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            if (transaction.Status == TransactionStatus.APPROVED)
                throw new DefaultException(MessageConstants.TRANSACTION_ALREADY_APPROVED);

            transaction.Status = TransactionStatus.APPROVED;
            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_APPROVED_SUCCESS
            };
        }
        public async Task<BaseResultModel> RejectTransactionAsync(Guid id)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(id);
            if (transaction == null)
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            if (transaction.Status == TransactionStatus.REJECTED)
                throw new DefaultException(MessageConstants.TRANSACTION_ALREADY_REJECTED);

            transaction.Status = TransactionStatus.REJECTED;
            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_REJECTED_SUCCESS
            };
        }
    }
}
