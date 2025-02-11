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
        private readonly IClaimsService _claimsService;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

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
                filter: t => t.UserId == user.Id
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

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_FETCHED_SUCCESS,
                Data = _mapper.Map<TransactionModel>(transaction)
            };
        }
        public async Task<BaseResultModel> CreateTransactionAsync(CreateTransactionModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            if (string.IsNullOrEmpty(userEmail))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.TRANSACTION_CREATE_DENIED,
                    Message = "Unauthorized: You must be logged in to create a transaction."
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

            if (model.TransactionDate == default)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.TRANSACTION_DATE_REQUIRED,
                    Message = "Transaction date is required."
                };
            }

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

            var transaction = _mapper.Map<Transaction>(model);
            transaction.UserId = user.Id;
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
            string userEmail = _claimsService.GetCurrentUserEmail;
            if (string.IsNullOrEmpty(userEmail))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.TRANSACTION_UPDATE_DENIED,
                    Message = "Unauthorized: You must be logged in to update a transaction."
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

            if (model.Id == Guid.Empty)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.TRANSACTION_ID_REQUIRED,
                    Message = "Transaction ID is required."
                };
            }

            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.Id);

            if (transaction == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.TRANSACTION_NOT_FOUND,
                    Message = "Transaction not found."
                };
            }

            if (transaction.UserId != user.Id)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.TRANSACTION_UPDATE_DENIED,
                    Message = "Access denied: You can only update your own transactions."
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
            if (model.TransactionDate == default)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.TRANSACTION_DATE_REQUIRED,
                    Message = "Transaction date is required."
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

            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(transactionId);
            if (transaction == null || transaction.UserId != user.Id)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.TRANSACTION_DELETE_DENIED,
                    Message = "Access denied: You can only delete your own transactions."
                };
            }

            _unitOfWork.TransactionsRepository.PermanentDeletedAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_DELETED_SUCCESS
            };
        }

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

    }
}
