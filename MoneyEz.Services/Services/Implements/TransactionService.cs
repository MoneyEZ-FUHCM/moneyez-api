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

        public async Task<BaseResultModel> GetAllTransactionsForUserAsync(Guid userId, PaginationParameter paginationParameter)
        {
            var transactions = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query.Include(t => t.Subcategory),
                filter: t => t.UserId == userId
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
        public async Task<BaseResultModel> GetTransactionByIdAsync(Guid transactionId)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdIncludeAsync(
                transactionId,
                include: query => query.Include(t => t.Subcategory)
            );

            if (transaction == null)
            {
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_FETCHED_SUCCESS,
                Data = _mapper.Map<TransactionModel>(transaction)
            };
        }
        public async Task<BaseResultModel> CreateTransactionAsync(CreateTransactionModel model)
        {
            var transaction = _mapper.Map<Transaction>(model);
            transaction.Status = TransactionStatus.APPROVED; // Mặc định là Approved
            await _unitOfWork.TransactionsRepository.AddAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.TRANSACTION_CREATED_SUCCESS
            };
        }
        public async Task<BaseResultModel> UpdateTransactionAsync(UpdateTransactionModel model)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.Id);

            if (transaction == null)
            {
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);
            }

            _mapper.Map(model, transaction);
            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_UPDATED_SUCCESS
            };
        }
        public async Task<BaseResultModel> DeleteTransactionAsync(Guid transactionId)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(transactionId);

            if (transaction == null)
            {
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);
            }

            _unitOfWork.TransactionsRepository.PermanentDeletedAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_DELETED_SUCCESS
            };
        }
    }
}
