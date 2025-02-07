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
            if (model.UserId == Guid.Empty)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TRANSACTION_REQUEST,
                    Message = "User ID is required."
                };
            }

            if (model.Amount <= 0)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TRANSACTION_REQUEST,
                    Message = "Amount must be greater than zero."
                };
            }

            if (!Enum.IsDefined(typeof(TransactionType), model.Type))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TRANSACTION_REQUEST,
                    Message = "Transaction type is invalid."
                };
            }

            if (model.SubcategoryId == Guid.Empty)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TRANSACTION_REQUEST,
                    Message = "Subcategory ID is required."
                };
            }

            if (model.Date == default)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TRANSACTION_REQUEST,
                    Message = "Transaction date is required."
                };
            }

            // Kiểm tra UserId có tồn tại trong DB không
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(model.UserId);
            if (user == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.USER_NOT_FOUND,
                    Message = "User does not exist."
                };
            }

            // Kiểm tra SubcategoryId có tồn tại trong DB không
            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId);
            if (subcategory == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND,
                    Message = "Subcategory does not exist."
                };
            }

            // Tạo giao dịch mới
            var transaction = _mapper.Map<Transaction>(model);
            transaction.Status = TransactionStatus.APPROVED; 

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
            if (model.Id == Guid.Empty)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.TRANSACTION_ID_REQUIRED,
                    Message = "Transaction ID is required."
                };
            }

            if (model.Amount <= 0)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.TRANSACTION_AMOUNT_REQUIRED,
                    Message = "Amount must be greater than zero."
                };
            }

            if (!Enum.IsDefined(typeof(TransactionType), model.Type))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.TRANSACTION_TYPE_INVALID,
                    Message = "Transaction type is invalid."
                };
            }

            if (model.SubcategoryId == Guid.Empty)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.TRANSACTION_SUBCATEGORY_REQUIRED,
                    Message = "Subcategory ID is required."
                };
            }

            if (model.Date == default)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.TRANSACTION_DATE_REQUIRED,
                    Message = "Transaction date is required."
                };
            }

            // Kiểm tra giao dịch có tồn tại không
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.Id);
            if (transaction == null)
            {
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);
            }

            // Kiểm tra danh mục con có tồn tại không
            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId);
            if (subcategory == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND,
                    Message = "Subcategory does not exist."
                };
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
