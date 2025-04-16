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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService, ITransactionNotificationService transactionNotificationService, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
            _transactionNotificationService = transactionNotificationService;
            _httpContextAccessor = httpContextAccessor;
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

        #region group
        public async Task<BaseResultModel> GetTransactionByGroupIdAsync(Guid groupId, PaginationParameter paginationParameter, TransactionFilter transactionFilter)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                groupId,
                include: q => q.Include(g => g.GroupMembers)
                             .Include(g => g.Transactions)
            );

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            var isMember = groupFund.GroupMembers.Any(member =>
                member.UserId == user.Id &&
                member.Status == GroupMemberStatus.ACTIVE);

            if (!isMember)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            var transactions = await _unitOfWork.TransactionsRepository.GetTransactionsFilterAsync(
                paginationParameter,
                transactionFilter,
                condition: t => t.GroupId == groupId,
                include: query => query.Include(t => t.Subcategory).Include(t => t.User)
            );

            var transactionModels = _mapper.Map<List<GroupTransactionModel>>(transactions);

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
                Message = MessageConstants.TRANSACTION_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> GetGroupTransactionDetailsAsync(Guid transactionId)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdIncludeAsync(transactionId,
                query => query.Include(t => t.User).Include(t => t.Group));

            if (transaction == null)
            {
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);
            }

            var transactionModel = _mapper.Map<TransactionModel>(transaction);

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, EntityName.TRANSACTION.ToString());
            transactionModel.Images = images.Select(i => i.ImageUrl).ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = transactionModel
            };
        }

        public async Task<BaseResultModel> CreateGroupTransactionAsync(CreateGroupTransactionModel model, string currentEmail)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(currentEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var group = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                model.GroupId, q => q.Include(g => g.GroupMembers).Include(g => g.GroupFundLogs))
                ?? throw new NotExistException(MessageConstants.GROUP_NOT_EXIST);

            var groupBankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(group.AccountBankId.Value);
            if (groupBankAccount == null)
            {
                throw new NotExistException("", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            var groupMember = group.GroupMembers.FirstOrDefault(m => m.UserId == user.Id)
                ?? throw new DefaultException(MessageConstants.USER_NOT_IN_GROUP);

            if (model.Amount <= 0)
                throw new DefaultException("Số tiền giao dịch phải lớn hơn 0.");

            if (!Enum.IsDefined(typeof(TransactionType), model.Type))
                throw new DefaultException("Loại giao dịch không hợp lệ.");

            //var now = CommonUtils.GetCurrentTime().Date;
            //if (model.TransactionDate.Date > now)
            //    throw new DefaultException("Không được tạo giao dịch cho ngày trong tương lai.");

            //if (model.TransactionDate < now.AddYears(-5) || model.TransactionDate > now.AddMonths(1))
            //    throw new DefaultException("Ngày giao dịch không hợp lệ.");

            if (model.Description?.Length > 1000)
                throw new DefaultException("Mô tả giao dịch quá dài (tối đa 1000 ký tự).");

            bool requiresApproval = groupMember.Role != RoleGroup.LEADER;
            TransactionStatus transactionStatus = requiresApproval ? TransactionStatus.PENDING : TransactionStatus.APPROVED;
            if (!requiresApproval)
            {
                model.TransactionDate = CommonUtils.GetCurrentTime();
            }
            else
            {
                model.TransactionDate = null;
            }

            // Generate random 10-digit code
            var requestCode = StringUtils.GenerateRandomUppercaseString(8);

            // Format final request code with bank short name
            var finalRequestCode = model.Type == TransactionType.INCOME ? $"GOPQUY-{requestCode}" : $"RUTQUY-{requestCode}";

            var transaction = _mapper.Map<Transaction>(model);
            transaction.UserId = user.Id;
            transaction.Status = transactionStatus;
            transaction.ApprovalRequired = requiresApproval;
            transaction.CreatedBy = user.Email;
            transaction.RequestCode = finalRequestCode;

            await _unitOfWork.TransactionsRepository.AddAsync(transaction);
            await _unitOfWork.SaveAsync();

            //string action = transaction.Type == TransactionType.INCOME ? "góp quỹ" : "rút quỹ";
            //string message = $"{user.FullName} đã tạo yêu cầu {action}: {transaction.Description}.";

            //await LogGroupFundChange(group, message, GroupAction.CREATED, user.Email);

            if (model.Images?.Any() == true)
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

            if (requiresApproval)
            {
                await _transactionNotificationService.NotifyTransactionApprovalRequestAsync(group, transaction, user);
            }
            else
            {
                // update financial goal and balance
                await UpdateFinancialGoalAndBalance(transaction, transaction.Amount);
                await _transactionNotificationService.NotifyTransactionCreatedAsync(group, transaction, user);
            }


            if (transaction.Type == TransactionType.INCOME)
            {
                // get info transaction fundraising request

                var response = new FundraisingTransactionResponse
                {
                    RequestCode = transaction.RequestCode,
                    Amount = transaction.Amount,
                    Status = transaction.Status.ToString(),
                    BankAccount = _mapper.Map<BankAccountModel>(groupBankAccount)
                };
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Fundraising request created successfully",
                    Data = response
                };
            }
            else
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Withdraw request created successfully",
                    Data = _mapper.Map<TransactionModel>(transaction)
                };
            }
        }

        public async Task<BaseResultModel> UpdateGroupTransactionAsync(UpdateGroupTransactionModel model)
        {
            if (model.Id == Guid.Empty)
                throw new DefaultException("Mã giao dịch không hợp lệ.");

            if (model.GroupId == Guid.Empty)
                throw new DefaultException("Mã nhóm không hợp lệ.");

            if (model.Amount.HasValue && model.Amount <= 0)
                throw new DefaultException("Số tiền phải lớn hơn 0.");

            if (model.Type.HasValue && !Enum.IsDefined(typeof(TransactionType), model.Type.Value))
                throw new DefaultException("Loại giao dịch không hợp lệ.");

            if (model.TransactionDate.HasValue)
            {
                var today = CommonUtils.GetCurrentTime().Date;
                if (model.TransactionDate.Value.Date > today)
                    throw new DefaultException("Không được cập nhật giao dịch cho ngày trong tương lai.");

                if (model.TransactionDate.Value.Date < today.AddYears(-5))
                    throw new DefaultException("Ngày giao dịch quá xa trong quá khứ.");
            }

            if (!string.IsNullOrWhiteSpace(model.Description) && model.Description.Length > 1000)
                throw new DefaultException("Mô tả giao dịch quá dài (tối đa 1000 ký tự).");

            if (model.Images != null && model.Images.Any(url => string.IsNullOrWhiteSpace(url)))
                throw new DefaultException("Ảnh đính kèm không được rỗng hoặc trống.");

            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.Id)
                ?? throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            await UpdateFinancialGoalAndBalance(transaction, -transaction.Amount);
            _mapper.Map(model, transaction);
            await UpdateFinancialGoalAndBalance(transaction, model.Amount ?? transaction.Amount);

            var group = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(transaction.GroupId.Value, q => q.Include(g => g.GroupFundLogs));
            await LogGroupFundChange(group, $"Giao dịch '{transaction.Description}' đã được cập nhật.", GroupAction.TRANSACTION_UPDATED, transaction.UpdatedBy);

            var oldImages = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, EntityName.TRANSACTION.ToString());
            _unitOfWork.ImageRepository.PermanentDeletedListAsync(oldImages);

            if (model.Images?.Any() == true)
            {
                var newImages = model.Images.Select(url => new Image
                {
                    EntityId = transaction.Id,
                    EntityName = EntityName.TRANSACTION.ToString(),
                    ImageUrl = url
                }).ToList();

                await _unitOfWork.ImageRepository.AddRangeAsync(newImages);
            }

            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_UPDATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> DeleteGroupTransactionAsync(Guid transactionId)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(transactionId)
                ?? throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            await UpdateFinancialGoalAndBalance(transaction, -transaction.Amount);

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, EntityName.TRANSACTION.ToString());
            if (images.Any())
            {
                _unitOfWork.ImageRepository.PermanentDeletedListAsync(images);
            }

            var group = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(transaction.GroupId.Value, q => q.Include(g => g.GroupFundLogs));
            await LogGroupFundChange(group, $"Giao dịch '{transaction.Description}' đã bị xóa.", GroupAction.TRANSACTION_DELETED, transaction.UpdatedBy);

            _unitOfWork.TransactionsRepository.PermanentDeletedAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_DELETED_SUCCESS
            };
        }

        public async Task<BaseResultModel> ResponseGroupTransactionAsync(ResponseGroupTransactionModel model)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.TransactionId)
                ?? throw new NotExistException("", MessageConstants.TRANSACTION_NOT_FOUND);

            var userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);

            var group = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(transaction.GroupId.Value, include: gr => gr.Include(x => x.GroupMembers))
                ?? throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);

            var groupMember = group.GroupMembers.FirstOrDefault(m => m.UserId == user.Id)
                ?? throw new DefaultException("", MessageConstants.USER_NOT_IN_GROUP);

            if (groupMember.Role != RoleGroup.LEADER)
                throw new DefaultException("", MessageConstants.PERMISSION_DENIED);

            if (model.IsApprove)
            {
                transaction.Status = TransactionStatus.APPROVED;
                transaction.TransactionDate = CommonUtils.GetCurrentTime();

                _unitOfWork.TransactionsRepository.UpdateAsync(transaction);
                await _unitOfWork.SaveAsync();

                await UpdateFinancialGoalAndBalance(transaction, transaction.Amount);

                await _transactionNotificationService.NotifyTransactionApprovalRequestAsync(group, transaction, user);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.TRANSACTION_APPROVED_SUCCESS
                };
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.Note))
                    throw new DefaultException("Reason for rejection cannot be left blank.", MessageConstants.TRANSACTION_REJECTED_MISSING_REASON);

                transaction.Status = TransactionStatus.REJECTED;
                transaction.TransactionDate = CommonUtils.GetCurrentTime();
                _unitOfWork.TransactionsRepository.UpdateAsync(transaction);
                await _unitOfWork.SaveAsync();

                await _transactionNotificationService.NotifyTransactionApprovalRequestAsync(group, transaction, user);

                string transactionContext = transaction.Type == TransactionType.INCOME ? "góp quỹ" : "rút quỹ";

                // get info transaction fundraising request
                var userRequest = await _unitOfWork.UsersRepository.GetByIdAsync(transaction.UserId.Value);
                if (userRequest != null)
                {
                    await LogGroupFundChange(group, $"Giao dịch {transactionContext} [{transaction.Description}] của [{userRequest.FullName}] đã bị từ chối. " +
                        $"\n[Lí do:] {model.Note}",
                        GroupAction.TRANSACTION_UPDATED, userEmail);
                }
                else
                {
                    throw new NotExistException("Not found user created transaction", MessageConstants.ACCOUNT_NOT_EXIST);
                }

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.TRANSACTION_REJECTED_SUCCESS
                };
            }
        }

        public async Task<BaseResultModel> RejectGroupTransactionAsync(Guid transactionId)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(transactionId)
                ?? throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            var userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var group = await _unitOfWork.GroupFundRepository.GetByIdAsync(transaction.GroupId.Value)
                ?? throw new NotExistException(MessageConstants.GROUP_NOT_EXIST);

            var groupMember = group.GroupMembers.FirstOrDefault(m => m.UserId == user.Id)
                ?? throw new DefaultException(MessageConstants.USER_NOT_IN_GROUP);

            if (groupMember.Role != RoleGroup.LEADER)
                throw new DefaultException(MessageConstants.PERMISSION_DENIED);

            transaction.Status = TransactionStatus.REJECTED;
            _unitOfWork.TransactionsRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveAsync();

            await _transactionNotificationService.NotifyTransactionApprovalRequestAsync(group, transaction, user);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.TRANSACTION_REJECTED_SUCCESS
            };
        }

        #region vote

        public async Task<BaseResultModel> CreateGroupTransactionVoteAsync(CreateGroupTransactionVoteModel model)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(model.TransactionId)
                ?? throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var existingVote = await _unitOfWork.TransactionVoteRepository.GetByConditionAsync(
                filter: v => v.TransactionId == model.TransactionId && v.UserId == user.Id);

            if (existingVote.Any())
            {
                throw new DefaultException(MessageConstants.VOTE_ALREADY_EXISTS);
            }

            var vote = new TransactionVote
            {
                TransactionId = model.TransactionId,
                UserId = user.Id,
                Vote = model.Vote
            };

            await _unitOfWork.TransactionVoteRepository.AddAsync(vote);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.VOTE_SUCCESS
            };
        }

        public async Task<BaseResultModel> UpdateGroupTransactionVoteAsync(UpdateGroupTransactionVoteModel model)
        {
            var vote = await _unitOfWork.TransactionVoteRepository.GetByIdAsync(model.Id)
                ?? throw new NotExistException(MessageConstants.VOTE_NOT_FOUND);

            vote.Vote = model.Vote;

            _unitOfWork.TransactionVoteRepository.UpdateAsync(vote);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.VOTE_UPDATED
            };
        }

        public async Task<BaseResultModel> DeleteGroupTransactionVoteAsync(Guid voteId)
        {
            var vote = await _unitOfWork.TransactionVoteRepository.GetByIdAsync(voteId)
                ?? throw new NotExistException(MessageConstants.VOTE_NOT_FOUND);

            _unitOfWork.TransactionVoteRepository.PermanentDeletedAsync(vote);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.VOTE_DELETED
            };
        }

        #endregion vote


        private async Task UpdateFinancialGoalAndBalance(Transaction transaction, decimal amount)
        {
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdAsync(transaction.GroupId.Value)
                ?? throw new NotExistException("GroupFund không tồn tại.");

            FinancialGoal? activeGoal = null;

            if (transaction.GroupId != Guid.Empty)
            {
                activeGoal = await _unitOfWork.FinancialGoalRepository
                    .GetActiveGoalByGroupId(transaction.GroupId.Value);
            }

            if (activeGoal == null && transaction.UserId.HasValue && transaction.SubcategoryId.HasValue)
            {
                activeGoal = await _unitOfWork.FinancialGoalRepository
                    .GetActiveGoalByUserAndSubcategory(transaction.UserId.Value, transaction.SubcategoryId.Value);
            }

            if (activeGoal != null && activeGoal.Status == FinancialGoalStatus.ACTIVE && activeGoal.Deadline > DateTime.UtcNow)
            {
                activeGoal.CurrentAmount += amount;

                if (activeGoal.CurrentAmount >= activeGoal.TargetAmount)
                {
                    activeGoal.CurrentAmount = activeGoal.TargetAmount;
                    activeGoal.Status = FinancialGoalStatus.COMPLETED;

                    // get user
                    var user = await _unitOfWork.UsersRepository.GetByIdAsync(activeGoal.UserId);

                    await _transactionNotificationService.NotifyGoalAchievedAsync(user, activeGoal);
                }

                _unitOfWork.FinancialGoalRepository.UpdateAsync(activeGoal);
            }

            if (transaction.Type == TransactionType.INCOME)
            {
                groupFund.CurrentBalance += amount;
            }
            else if (transaction.Type == TransactionType.EXPENSE)
            {
                groupFund.CurrentBalance -= amount;
            }

            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();
        }

        private async Task LogGroupFundChange(GroupFund group, string description, GroupAction action, string userEmail)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException("User not found", MessageConstants.ACCOUNT_NOT_EXIST);
            var log = new GroupFundLog
            {
                GroupId = group.Id,
                ChangedBy = user.FullName,
                ChangeDescription = description,
                Action = action.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = user.Email,
            };

            await _unitOfWork.GroupFundLogRepository.AddAsync(log);
            await _unitOfWork.SaveAsync();
        }


        #endregion group

        #region python webhook
        public async Task<BaseResultModel> UpdateTransactionWebhook(WebhookPayload webhookPayload)
        {
            // Get bank account to validate secret key
            var bankAccount = await _unitOfWork.BankAccountRepository.GetBankAccountByNumberAsync(webhookPayload.AccountNumber);

            if (bankAccount == null || string.IsNullOrEmpty(bankAccount.WebhookSecretKey))
            {
                throw new NotExistException("Bank account not found or missing webhook configuration",
                    MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // Get secret key from header
            var secretKey = _httpContextAccessor.HttpContext?.Request.Headers["X-Webhook-Secret"].ToString();

            if (string.IsNullOrEmpty(secretKey) || secretKey != bankAccount.WebhookSecretKey)
            {
                throw new DefaultException("Invalid webhook secret key", MessageConstants.INVALID_WEBHOOK_SECRET);
            }

            // kiểm tra số tài khoản ngân hàng đã được liên kết với nhóm chưa
            GroupFund groupBankAccount = null;
            var groupFunds = await _unitOfWork.GroupFundRepository.GetByAccountBankId(bankAccount.Id);
            if (groupFunds.Any())
            {
                groupBankAccount = groupFunds.First();
            }

            // lấy thông tin người dùng (chủ tài khoản)
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(bankAccount.UserId);
            if (user == null)
            {
                throw new NotExistException("User not found", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // lấy transaction by request code (nếu có thì cập nhật trạng thái / nếu không thì tạo mới)
            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.RequestCode == webhookPayload.Description);
            var updatedTransactions = transactions.FirstOrDefault();

            // trường hợp có giao dịch trùng với request code
            if (updatedTransactions != null)
            {
                // trường hợp đã liên kết ngân hàng với nhóm
                if (groupBankAccount != null)
                {
                    // trường hợp số tiền giao dịch hợp lệ
                    if (updatedTransactions.Amount == webhookPayload.Amount)
                    {
                        // cập nhật lại giao dịch đã có trước đó (từ việc góp quỹ, rút quỹ)
                        updatedTransactions.Status = TransactionStatus.APPROVED;
                        updatedTransactions.UpdatedBy = user.Email;
                        updatedTransactions.BankTransactionId = webhookPayload.TransactionId;
                        updatedTransactions.TransactionDate = webhookPayload.Timestamp;
                        updatedTransactions.AccountBankNumber = webhookPayload.AccountNumber;
                        updatedTransactions.AccountBankName = webhookPayload.BankName;

                        _unitOfWork.TransactionsRepository.UpdateAsync(updatedTransactions);
                        await _unitOfWork.SaveAsync();
                    }
                    else
                    {
                        // trường hợp giao dịch số tiền không hợp lệ
                        // tạo transaction mới cho group
                        var newTransactionGroup = new CreateGroupTransactionModel
                        {
                            Amount = webhookPayload.Amount,
                            Description = "[Ngân hàng] " + webhookPayload.Description,
                            Type = webhookPayload.TransactionType,
                            TransactionDate = webhookPayload.Timestamp,
                            GroupId = groupBankAccount.Id,
                            InsertType = InsertType.BANKING,
                            AccountBankNumber = webhookPayload.AccountNumber,
                            AccountBankName = webhookPayload.BankName,
                            BankTransactionDate = webhookPayload.Timestamp,
                            BankTransactionId = webhookPayload.TransactionId
                        };

                        return await CreateGroupTransactionAsync(newTransactionGroup, user.Email);
                    }
                }
                else
                {
                    // trường hợp không liên kết ngân hàng với nhóm
                    // tạo mới transaction cho user
                    // chỉ hỗ trợ thêm giao dịch vào nếu user đã có mô hình chi tiêu

                    // search user spending model
                    var userSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(user.Id);
                    if (userSpendingModel == null)
                    {
                        throw new NotExistException("User spending model not found", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);
                    }

                    // create new transaction
                    var newTransaction = new Transaction
                    {
                        Amount = webhookPayload.Amount,
                        Description = "[Ngân hàng] " + webhookPayload.Description,
                        Status = TransactionStatus.PENDING,
                        Type = webhookPayload.TransactionType,
                        TransactionDate = webhookPayload.Timestamp,
                        UserId = bankAccount.UserId,
                        CreatedBy = user.Email,
                        ApprovalRequired = false,
                        InsertType = InsertType.BANKING,
                        UserSpendingModelId = userSpendingModel.Id,
                        BankTransactionDate = webhookPayload.Timestamp,
                        BankTransactionId = webhookPayload.TransactionId,
                        AccountBankNumber = webhookPayload.AccountNumber,
                        AccountBankName = webhookPayload.BankName
                    };

                    // tự phân loại giao dịch với tiền lương
                    var isSalary = StringUtils.IsDescriptionContainsSalaryKeywords(webhookPayload.Description);
                    if (isSalary)
                    {
                        var salarySubcategory = await _unitOfWork.SubcategoryRepository.GetByConditionAsync(
                            filter: sc => sc.Code == "sc-luong" && !sc.IsDeleted);
                        if (salarySubcategory.Any())
                        {
                            newTransaction.SubcategoryId = salarySubcategory.First().Id;
                        }
                    }

                    await _unitOfWork.TransactionsRepository.AddAsync(newTransaction);
                    await _unitOfWork.SaveAsync();
                }
            }
            else
            {
                // trường hợp không liên kết ngân hàng với nhóm
                // tạo mới transaction cho user
                // chỉ hỗ trợ thêm giao dịch vào nếu user đã có mô hình chi tiêu

                // search user spending model
                var userSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(user.Id);
                if (userSpendingModel == null)
                {
                    throw new NotExistException("User spending model not found", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);
                }

                // create new transaction
                var newTransaction = new Transaction
                {
                    Amount = webhookPayload.Amount,
                    Description = "[Ngân hàng] " + webhookPayload.Description,
                    Status = TransactionStatus.PENDING,
                    Type = webhookPayload.TransactionType,
                    TransactionDate = webhookPayload.Timestamp,
                    UserId = bankAccount.UserId,
                    CreatedBy = user.Email,
                    ApprovalRequired = false,
                    InsertType = InsertType.BANKING,
                    UserSpendingModelId = userSpendingModel.Id,
                    BankTransactionDate = webhookPayload.Timestamp,
                    BankTransactionId = webhookPayload.TransactionId,
                    AccountBankNumber = webhookPayload.AccountNumber,
                    AccountBankName = webhookPayload.BankName
                };

                // tự phân loại giao dịch với tiền lương
                var isSalary = StringUtils.IsDescriptionContainsSalaryKeywords(webhookPayload.Description);
                if (isSalary)
                {
                    var salarySubcategory = await _unitOfWork.SubcategoryRepository.GetByConditionAsync(
                        filter: sc => sc.Code == "sc-luong" && !sc.IsDeleted);
                    if (salarySubcategory.Any())
                    {
                        newTransaction.SubcategoryId = salarySubcategory.First().Id;
                    }
                }

                await _unitOfWork.TransactionsRepository.AddAsync(newTransaction);
                await _unitOfWork.SaveAsync();
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Transaction status updated successfully"
            };

        }

        public async Task<BaseResultModel> CreateTransactionPythonService(CreateTransactionPythonModel model)
        {
            // get info user
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(model.UserId);
            if (user == null)
            {
                throw new NotExistException("User not found", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // search subcategory
            var subcategory = await _unitOfWork.SubcategoryRepository.GetByConditionAsync(filter: sc => sc.Code == model.SubcategoryCode && !sc.IsDeleted);
            if (!subcategory.Any())
            {
                throw new NotExistException("Subcategory not found", MessageConstants.SUBCATEGORY_NOT_FOUND);
            }

            var newTransaction = new CreateTransactionModel
            {
                Amount = model.Amount,
                Description = model.Description,
                SubcategoryId = subcategory.First().Id,
                TransactionDate = CommonUtils.GetCurrentTime()
            };

            return await CreateTransactionAsync(newTransaction, user.Email);
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

                return await UpdateGroupTransactionAsync(updateGroupTransaction);
            }
        }
        #endregion python webhook

        #region report

        public async Task<BaseResultModel> GetYearReportAsync(int year, ReportTransactionType type)
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

            var monthly = new List<MonthAmountModel>();

            for (int month = 1; month <= 12; month++)
            {
                var monthTransactions = transactions
                    .Where(t => t.TransactionDate!.Value.Month == month)
                    .AsQueryable();

                var value = type == ReportTransactionType.Total
                    ? GetIncomeExpenseTotal(monthTransactions).total
                    : GetTotalByType(monthTransactions, type);

                monthly.Add(new MonthAmountModel
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                    Amount = value
                });
            }

            decimal total = type == ReportTransactionType.Total
                ? GetIncomeExpenseTotal(transactions.AsQueryable()).total
                : GetTotalByType(transactions.AsQueryable(), type);

            var currentMonth = DateTime.Now.Month;
            var monthsElapsed = (year == DateTime.Now.Year) ? currentMonth : 12;
            var avg = monthsElapsed > 0 ? total / monthsElapsed : 0;

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new YearTransactionReportModel
                {
                    Year = year,
                    Type = type,
                    Total = total,
                    Average = avg,
                    MonthlyData = monthly
                }
            };
        }

        public async Task<BaseResultModel> GetCategoryYearReportAsync(int year, ReportTransactionType type)
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

            var filtered = _unitOfWork.TransactionsRepository
                .FilterByType(transactions.AsQueryable(), type);

            decimal total = type == ReportTransactionType.Total
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
                    Type = type,
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

        public async Task<BaseResultModel> GetAllTimeCategoryReportAsync(ReportTransactionType type)
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

            var filtered = _unitOfWork.TransactionsRepository
                .FilterByType(transactions.AsQueryable(), type);

            decimal total = type == ReportTransactionType.Total
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
                    Type = type,
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

        #endregion report

        #region helper
        private (decimal income, decimal expense, decimal total) GetIncomeExpenseTotal(IQueryable<Transaction> source)
        {
            var income = _unitOfWork.TransactionsRepository
                .FilterByType(source, ReportTransactionType.Income)
                .Sum(t => t.Amount);

            var expense = _unitOfWork.TransactionsRepository
                .FilterByType(source, ReportTransactionType.Expense)
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

        #endregion helper
    }
}
