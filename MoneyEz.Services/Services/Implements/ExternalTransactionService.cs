using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.BusinessModels.TransactionModels.Group;
using MoneyEz.Services.BusinessModels.WebhookModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class ExternalTransactionService : IExternalTransactionService
    {
        private IUnitOfWork _unitOfWork;
        private readonly IGroupTransactionService _groupTransactionService;
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExternalTransactionService(IUnitOfWork unitOfWork,
            IGroupTransactionService groupTransactionService,
            ITransactionService transactionService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _groupTransactionService = groupTransactionService;
            _transactionService = transactionService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
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

            return await _transactionService.CreateTransactionAsync(newTransaction, user.Email);
        }

        public async Task<BaseResultModel> CreateTransactionPythonServiceV2(CreateTransactionPythonModelV2 model)
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
                TransactionDate = model.TransactionDate,
            };

            return await _transactionService.CreateTransactionAsync(newTransaction, user.Email);
        }

        public async Task<BaseResultModel> GetTransactionHistorySendToPythons(Guid userId, TransactionFilter transactionFilter)
        {
            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == userId && t.GroupId == null
                    && (!transactionFilter.FromDate.HasValue || t.TransactionDate.Value.Date >= transactionFilter.FromDate.Value.Date)
                    && (!transactionFilter.ToDate.HasValue || t.TransactionDate.Value.Date <= transactionFilter.ToDate.Value.Date)
                    && !t.IsDeleted && t.Status == TransactionStatus.APPROVED,
                include: q => q.Include(t => t.Subcategory),
                orderBy: t => t.OrderByDescending(t => t.TransactionDate)
            );
            var transactionModels = _mapper.Map<List<TransactionHistorySendToPython>>(transactions);
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = transactionModels
            };
        }

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

                        //await _groupTransactionService.UpdateFinancialGoalAndBalance(updatedTransactions, updatedTransactions.Amount);
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

                        return await _groupTransactionService.CreateGroupTransactionAsync(newTransactionGroup, user.Email);
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
    }
}
