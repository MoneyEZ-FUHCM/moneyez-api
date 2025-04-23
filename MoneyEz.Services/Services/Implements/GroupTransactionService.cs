using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Services.BusinessModels.GroupFund;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.TransactionModels.Group;
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
    public class GroupTransactionService : IGroupTransactionService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly INotificationService _notificationService;
        private readonly ITransactionService _transactionService;

        public GroupTransactionService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            INotificationService notificationService,
            ITransactionService transactionService) 
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _notificationService = notificationService;
            _transactionService = transactionService;
        }
        /// <summary>
        /// tạo yêu cầu góp quỹ vào nhóm
        /// 
        public async Task<BaseResultModel> CreateFundraisingRequest(CreateFundraisingModel createFundraisingModel)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Get group with bank account info
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                createFundraisingModel.GroupId,
                include: q => q.Include(g => g.GroupMembers));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            var groupBankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(groupFund.AccountBankId.Value);
            if (groupBankAccount == null)
            {
                throw new NotExistException("", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // Verify user is a member of the group
            var isMember = groupFund.GroupMembers.Any(member =>
                member.UserId == currentUser.Id &&
                member.Status == GroupMemberStatus.ACTIVE);

            if (!isMember)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            // Create a new fundraising request
            var newFundraisingRequest = new CreateGroupTransactionModel
            {
                GroupId = createFundraisingModel.GroupId,
                Description = createFundraisingModel.Description,
                Amount = createFundraisingModel.Amount,
                Type = TransactionType.INCOME
            };

            return await _transactionService.CreateGroupTransactionAsync(newFundraisingRequest, currentUser.Email);
        }

        /// <summary>
        /// tạo yêu cầu rút tiền từ quỹ nhóm
        /// 
        public async Task<BaseResultModel> CreateFundWithdrawalRequest(CreateFundWithdrawalModel createFundWithdrawalModel)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Get group with bank account info
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                createFundWithdrawalModel.GroupId,
                include: q => q.Include(g => g.GroupMembers));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // validate amount
            if (createFundWithdrawalModel.Amount > groupFund.CurrentBalance)
            {
                throw new DefaultException("Amount must be less than or equal to the balance",
                    MessageConstants.GROUP_WITHDRAWAL_AMOUNT_INVALID);
            }

            var groupBankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(groupFund.AccountBankId.Value);
            if (groupBankAccount == null)
            {
                throw new NotExistException("", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // Verify user is a member of the group
            var isMember = groupFund.GroupMembers.Any(member =>
                member.UserId == currentUser.Id &&
                member.Status == GroupMemberStatus.ACTIVE);

            if (!isMember)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            // Create a new fundraising request
            var newFundraisingRequest = new CreateGroupTransactionModel
            {
                GroupId = createFundWithdrawalModel.GroupId,
                Description = createFundWithdrawalModel.Description,
                Amount = createFundWithdrawalModel.Amount,
                Type = TransactionType.EXPENSE,
                Images = createFundWithdrawalModel.Images
            };

            return await _transactionService.CreateGroupTransactionAsync(newFundraisingRequest, currentUser.Email);
        }

        public async Task<BaseResultModel> GetPendingRequestDetailAsync(Guid requestId)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // get pending requests
            var pendingRequest = await _unitOfWork.TransactionsRepository.GetByIdIncludeAsync(
                requestId,
                filter: t => t.Status == TransactionStatus.PENDING,
                include: q => q.Include(t => t.User)
            );

            // get group fund
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(pendingRequest.GroupId.Value,
                include: g => g.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // get group bank account
            var groupBankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(groupFund.AccountBankId.Value);

            if (pendingRequest.Type == TransactionType.INCOME)
            {
                // get info transaction fundraising request
                var response = new
                {
                    Transaction = _mapper.Map<GroupTransactionModel>(pendingRequest),
                    BankAccount = _mapper.Map<BankAccountModel>(groupBankAccount)
                };
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Pending request get successfully",
                    Data = response
                };
            }
            else
            {
                var withdrawalRequest = _mapper.Map<GroupTransactionModel>(pendingRequest);
                
                // Calculate withdrawal limit and set note
                var (maxWithdrawalLimit, contributionPercentage) = 
                    await CalculateWithdrawalLimitAsync(groupFund, pendingRequest.UserId.Value);
                
                // Add withdrawal limit info to notes with contribution percentage
                withdrawalRequest.Note = maxWithdrawalLimit > 0 
                    ? $"Hạn mức rút tối đa (khuyến nghị) của thành viên là: {maxWithdrawalLimit:N0} VND (dựa trên {contributionPercentage:F2}% đóng góp vào quỹ)" 
                    : $"Thành viên chưa góp quỹ hoặc đã rút hết số tiền góp";

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Pending request get successfully",
                    Data = withdrawalRequest
                };
            }
        }

        public async Task<BaseResultModel> GetPendingRequestsAsync(Guid groupId, PaginationParameter paginationParameters)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // get group fund
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(groupId,
                include: g => g.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // check user is leader
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id
                && member.Role == RoleGroup.LEADER && member.Status == GroupMemberStatus.ACTIVE);

            if (isLeader)
            {
                // get all pending requests
                var pendingRequests = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                    paginationParameters,
                    filter: t => t.GroupId == groupId && t.Status == TransactionStatus.PENDING,
                    include: q => q.Include(t => t.User),
                    orderBy: q => q.OrderByDescending(t => t.CreatedDate)
                );

                var pendingRequestModels = _mapper.Map<List<GroupTransactionModel>>(pendingRequests);

                // For withdrawal transactions, calculate and add withdrawal limit to notes
                foreach (var request in pendingRequestModels.Where(r => r.Type == TransactionType.EXPENSE.ToString()))
                {
                    // Calculate withdrawal limit and set note
                    var (maxWithdrawalLimit, contributionPercentage) =
                        await CalculateWithdrawalLimitAsync(groupFund, request.UserId);

                    // Add withdrawal limit info to notes with contribution percentage
                    request.Note = maxWithdrawalLimit > 0
                        ? $"Hạn mức rút tối đa (khuyến nghị) của thành viên là: {maxWithdrawalLimit:N0} VND (dựa trên {contributionPercentage:F2}% đóng góp vào quỹ)"
                        : $"Thành viên chưa góp quỹ hoặc đã rút hết số tiền góp";
                }
                var result = PaginationHelper.GetPaginationResult(pendingRequests, pendingRequestModels);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = result
                };
            }
            else
            {
                // get users pending requests
                var pendingRequests = await _unitOfWork.TransactionsRepository.ToPaginationIncludeAsync(
                    paginationParameters,
                    filter: t => t.GroupId == groupId && t.UserId == currentUser.Id && t.Status == TransactionStatus.PENDING,
                    include: q => q.Include(t => t.User),
                    orderBy: q => q.OrderByDescending(t => t.CreatedDate)
                );

                var pendingRequestModels = _mapper.Map<List<GroupTransactionModel>>(pendingRequests);
                
                // For withdrawal transactions, calculate and add withdrawal limit to notes
                foreach (var request in pendingRequestModels.Where(r => r.Type == TransactionType.EXPENSE.ToString()))
                {
                    // Calculate withdrawal limit and set note
                    var (maxWithdrawalLimit, contributionPercentage) = 
                        await CalculateWithdrawalLimitAsync(groupFund, request.UserId);
                    
                    // Add withdrawal limit info to notes with contribution percentage
                    request.Note = maxWithdrawalLimit > 0 
                        ? $"Hạn mức rút tối đa (khuyến nghị) của thành viên là: {maxWithdrawalLimit:N0} VND (dựa trên {contributionPercentage:F2}% đóng góp vào quỹ)" 
                        : $"Thành viên chưa góp quỹ hoặc đã rút hết số tiền góp";
                }
                
                var result = PaginationHelper.GetPaginationResult(pendingRequests, pendingRequestModels);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = result,
                };
            }
        }

        /// <summary>
        /// tạo lời nhắc góp quỹ cho các thành viên trong nhóm (chỉ leader mới có quyền tạo)
        /// 
        public async Task<BaseResultModel> RemindFundraisingAsync(RemindFundraisingModel remindFundraisingModel)
        {
            // Get and verify current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Get group with members and financial goals
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(
                remindFundraisingModel.GroupId,
                include: q => q.Include(g => g.GroupMembers).Include(g => g.FinancialGoals));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Validate leader permission
            var isLeader = groupFund.GroupMembers.Any(member =>
                member.UserId == currentUser.Id &&
                member.Role == RoleGroup.LEADER &&
                member.Status == GroupMemberStatus.ACTIVE);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_REMIND_FORBIDDEN
                };
            }

            // valid amount
            if (remindFundraisingModel.Members.Any(m => m.Amount <= 0))
            {
                throw new DefaultException(MessageConstants.GROUP_REMIND_AMOUNT_INVALID_MESSAGE,
                    MessageConstants.GROUP_REMIND_AMOUNT_INVALID);
            }

            // Get active financial goal if exists
            var activeGoal = groupFund.FinancialGoals
                .FirstOrDefault(g => g.Status == FinancialGoalStatus.ACTIVE);

            // Process each member
            foreach (var memberRemind in remindFundraisingModel.Members)
            {
                var groupMember = groupFund.GroupMembers.FirstOrDefault(m =>
                    m.UserId == memberRemind.MemberId &&
                    m.Status == GroupMemberStatus.ACTIVE);

                if (groupMember == null)
                {
                    throw new NotExistException($"Member with ID {memberRemind.MemberId} not found in group",
                        MessageConstants.GROUP_MEMBER_NOT_FOUND);
                }

                // Get member user info
                var memberUser = await _unitOfWork.UsersRepository.GetByIdAsync(memberRemind.MemberId);
                if (memberUser == null)
                {
                    throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
                }

                // Send notification
                var notification = new Notification
                {
                    UserId = memberRemind.MemberId,
                    Title = $"Nhắc nhở góp quỹ [{groupFund.Name}]",
                    Message = $"Bạn cần đóng góp {memberRemind.Amount:N0} VNĐ cho quỹ nhóm '{groupFund.Name}",
                    EntityId = groupFund.Id,
                    Type = NotificationType.GROUP,
                };
                await _notificationService.AddNotificationByUserId(memberRemind.MemberId, notification);

                // create transaction pending
                var groupTransaction = new CreateGroupTransactionModel
                {
                    GroupId = groupFund.Id,
                    Description = $"Nhắc nhở góp quỹ cho thành viên {memberUser.FullName}",
                    Amount = memberRemind.Amount,
                    Type = TransactionType.INCOME
                };

                var transactionResult = await _transactionService.CreateGroupTransactionAsync(groupTransaction, memberUser.Email);
            }

            // Add group fund log
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã gửi nhắc nhở góp quỹ" + (activeGoal != null ? $" cho mục tiêu '{activeGoal.Name}'" : ""),
                Action = GroupAction.TRANSACTION_CREATED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            // Save changes
            groupFund.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_REMIND_SUCCESS_MESSAGE
            };
        }

        // Helper method to calculate member's current balance
        private async Task<decimal> CalculateMemberBalanceAsync(Guid groupId, Guid userId)
        {
            // Get all approved transactions for this member
            var memberTransactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.GroupId == groupId &&
                            t.UserId == userId &&
                            t.Status == TransactionStatus.APPROVED
            );

            // Calculate balance (deposits - withdrawals)
            decimal totalDeposits = memberTransactions
                .Where(t => t.Type == TransactionType.INCOME)
                .Sum(t => t.Amount);

            decimal totalWithdrawals = memberTransactions
                .Where(t => t.Type == TransactionType.EXPENSE)
                .Sum(t => t.Amount);

            return totalDeposits - totalWithdrawals;
        }
        
        /// <summary>
        /// Calculates the recommended withdrawal limit for a member based on their contribution percentage
        /// </summary>
        /// <param name="groupFund">The group fund</param>
        /// <param name="userId">The user ID of the member</param>
        /// <returns>A tuple containing (maxWithdrawalLimit, contributionPercentage)</returns>
        private async Task<(decimal maxWithdrawalLimit, decimal contributionPercentage)> CalculateWithdrawalLimitAsync(
            GroupFund groupFund, Guid userId)
        {
            // Calculate total deposits by the group member
            var memberTotalDeposits = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.GroupId == groupFund.Id &&
                            t.UserId == userId &&
                            t.Type == TransactionType.INCOME &&
                            t.Status == TransactionStatus.APPROVED
            );

            decimal totalDepositAmount = memberTotalDeposits.Sum(t => t.Amount);

            // Calculate current balance
            var memberBalance = await CalculateMemberBalanceAsync(groupFund.Id, userId);

            // Get all approved deposits for the group to calculate total contributions
            var allGroupDeposits = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.GroupId == groupFund.Id &&
                            t.Type == TransactionType.INCOME &&
                            t.Status == TransactionStatus.APPROVED
            );

            decimal totalGroupDepositAmount = allGroupDeposits.Sum(t => t.Amount);

            // Calculate contribution percentage
            decimal contributionPercentage = totalGroupDepositAmount > 0 
                ? totalDepositAmount / totalGroupDepositAmount * 100 
                : 0;

            // Calculate withdrawal limit based on contribution percentage
            decimal recommendedWithdrawalLimit = Math.Round(groupFund.CurrentBalance * (contributionPercentage / 100), 0);
            
            // Determine final maximum withdrawal limit
            // It should not exceed member's current balance or their total deposits
            decimal maxWithdrawalLimit = Math.Min(
                Math.Min(totalDepositAmount, memberBalance), 
                recommendedWithdrawalLimit
            );
            
            return (maxWithdrawalLimit, contributionPercentage);
        }
    }
}
