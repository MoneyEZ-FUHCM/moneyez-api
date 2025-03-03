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

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        //single user
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
                orderBy: t=> t.OrderByDescending(t => t.CreatedDate)
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

            var user = (await _unitOfWork.UsersRepository.GetByConditionAsync(
                filter: u => u.Email == userEmail
            )).FirstOrDefault();

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

            var currentSpendingModel = (await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id &&
                               usm.StartDate <= DateTime.UtcNow &&
                               (usm.EndDate == null || usm.EndDate >= DateTime.UtcNow)
            )).FirstOrDefault();

            if (currentSpendingModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SPENDING_MODEL_DATA_MISSING,
                    Message = "User must select a spending model before creating a transaction."
                };
            }

            var transaction = _mapper.Map<Transaction>(model);
            transaction.UserId = user.Id;
            transaction.ApprovalRequired = false;
            transaction.Status = TransactionStatus.APPROVED;

            await _unitOfWork.TransactionsRepository.AddAsync(transaction);

            var financialGoal = (await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.SubcategoryId == model.SubcategoryId &&
                              fg.UserId == user.Id &&
                              !fg.IsDeleted &&
                              fg.Deadline >= DateTime.UtcNow
            )).FirstOrDefault();

            if (financialGoal != null)
            {
                if (model.Type == TransactionType.EXPENSE)
                {
                    financialGoal.CurrentAmount += model.Amount;
                }
                else if (model.Type == TransactionType.INCOME)
                {
                    financialGoal.CurrentAmount += model.Amount;
                }

                _unitOfWork.FinancialGoalRepository.UpdateAsync(financialGoal);
            }

            if (model.Images != null && model.Images.Any())
            {
                var images = model.Images.Select(url => new Image
                {
                    EntityId = transaction.Id,
                    EntityName = "Transaction",
                    ImageUrl = url,
                    CreatedDate = DateTime.UtcNow
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

            var user = (await _unitOfWork.UsersRepository.GetByConditionAsync(
                filter: u => u.Email == userEmail
            )).FirstOrDefault();

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

            if (model.Amount.HasValue && model.Amount <= 0)
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

            var currentSpendingModel = (await _unitOfWork.UserSpendingModelRepository.GetByConditionAsync(
                filter: usm => usm.UserId == user.Id &&
                               usm.StartDate <= DateTime.UtcNow &&
                               (usm.EndDate == null || usm.EndDate >= DateTime.UtcNow)
            )).FirstOrDefault();

            if (currentSpendingModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SPENDING_MODEL_DATA_MISSING,
                    Message = "User must select a spending model before updating a transaction."
                };
            }

            var validCategory = (await _unitOfWork.SpendingModelCategoryRepository.GetByConditionAsync(
                filter: smc => smc.SpendingModelId == currentSpendingModel.SpendingModelId &&
                               smc.Category.CategorySubcategories.Any(cs => cs.SubcategoryId == model.SubcategoryId)
            )).FirstOrDefault();

            if (validCategory == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_IN_SPENDING_MODEL,
                    Message = "Selected subcategory is not allowed in the current spending model."
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

            var oldSubcategoryId = transaction.SubcategoryId;
            var oldAmount = transaction.Amount;
            var oldType = transaction.Type;

            // Cập nhật transaction theo model mới
            _mapper.Map(model, transaction);
            transaction.ApprovalRequired = false;
            transaction.Status = TransactionStatus.APPROVED;

            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);

            // Xóa và thêm lại ảnh
            var oldImages = await _unitOfWork.ImageRepository.GetByConditionAsync(
                filter: img => img.EntityId == transaction.Id && img.EntityName == "Transaction"
            );
            _unitOfWork.ImageRepository.PermanentDeletedListAsync(oldImages);

            if (model.Images != null && model.Images.Any())
            {
                var newImages = model.Images.Select(url => new Image
                {
                    EntityId = transaction.Id,
                    EntityName = "Transaction",
                    ImageUrl = url,
                    CreatedDate = DateTime.UtcNow
                }).ToList();

                await _unitOfWork.ImageRepository.AddRangeAsync(newImages);
            }

            // Xử lý FinancialGoal (2 trường hợp: Subcategory giữ nguyên hoặc đổi)
            if (oldSubcategoryId != model.SubcategoryId)
            {
                // Hoàn trả goal cũ
                var oldGoal = (await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                            filter: fg => fg.SubcategoryId == transaction.SubcategoryId &&
                                  fg.UserId == user.Id &&
                                  !fg.IsDeleted &&
                                  fg.Deadline >= DateTime.UtcNow
                            )).FirstOrDefault();

                if (oldGoal != null)
                {
                    if (oldType == TransactionType.EXPENSE)
                        oldGoal.CurrentAmount -= oldAmount;
                    else if (oldType == TransactionType.INCOME)
                        oldGoal.CurrentAmount -= oldAmount;

                    _unitOfWork.FinancialGoalRepository.UpdateAsync(oldGoal);
                }

                // Cộng goal mới
                var newGoal = (await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                    filter: fg => fg.SubcategoryId == model.SubcategoryId && fg.UserId == user.Id &&
                                  !fg.IsDeleted &&
                                  fg.Deadline >= DateTime.UtcNow
                )).FirstOrDefault();

                if (newGoal != null)
                {
                    var newAmount = model.Amount ?? transaction.Amount;
                    if (model.Type == TransactionType.EXPENSE)
                        newGoal.CurrentAmount += newAmount;
                    else if (model.Type == TransactionType.INCOME)
                        newGoal.CurrentAmount += newAmount;

                    _unitOfWork.FinancialGoalRepository.UpdateAsync(newGoal);
                }
            }
            else
            {
                // Không đổi subcategory -> cập nhật ngay trên goal hiện tại
                var currentGoal = (await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                    filter: fg => fg.SubcategoryId == model.SubcategoryId && fg.UserId == user.Id &&
                                  !fg.IsDeleted &&
                                  fg.Deadline >= DateTime.UtcNow
                )).FirstOrDefault();

                if (currentGoal != null)
                {
                    if (oldType == TransactionType.EXPENSE)
                        currentGoal.CurrentAmount -= oldAmount;
                    else if (oldType == TransactionType.INCOME)
                        currentGoal.CurrentAmount -= oldAmount;

                    var newAmount = model.Amount ?? transaction.Amount;
                    if (model.Type == TransactionType.EXPENSE)
                        currentGoal.CurrentAmount += newAmount;
                    else if (model.Type == TransactionType.INCOME)
                        currentGoal.CurrentAmount += newAmount;

                    _unitOfWork.FinancialGoalRepository.UpdateAsync(currentGoal);
                }
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

            var user = (await _unitOfWork.UsersRepository.GetByConditionAsync(
                filter: u => u.Email == userEmail
            )).FirstOrDefault();

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

            var financialGoal = (await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: fg => fg.SubcategoryId == transaction.SubcategoryId && fg.UserId == user.Id
            )).FirstOrDefault();

            if (financialGoal != null)
            {
                if (transaction.Type == TransactionType.EXPENSE)
                {
                    financialGoal.CurrentAmount -= transaction.Amount;
                }
                else if (transaction.Type == TransactionType.INCOME)
                {
                    financialGoal.CurrentAmount -= transaction.Amount;
                }

                _unitOfWork.FinancialGoalRepository.UpdateAsync(financialGoal);
            }

            var images = await _unitOfWork.ImageRepository.GetByConditionAsync(
                filter: img => img.EntityId == transaction.Id && img.EntityName == "Transaction"
            );

            _unitOfWork.ImageRepository.PermanentDeletedListAsync(images);

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
