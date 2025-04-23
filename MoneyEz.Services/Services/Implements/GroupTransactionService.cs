using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Services.BusinessModels.GroupFund;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.TransactionModels;
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
        private readonly ITransactionNotificationService _transactionNotificationService;

        public GroupTransactionService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            INotificationService notificationService,
            ITransactionNotificationService transactionNotificationService) 
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _notificationService = notificationService;
            _transactionNotificationService = transactionNotificationService;
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

            return await CreateGroupTransactionAsync(newFundraisingRequest, currentUser.Email);
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

            return await CreateGroupTransactionAsync(newFundraisingRequest, currentUser.Email);
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

                var transactionResult = await CreateGroupTransactionAsync(groupTransaction, memberUser.Email);
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

        public async Task<BaseResultModel> DeleteGroupTransactionAsync(Guid transactionId)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdAsync(transactionId)
                ?? throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);

            await UpdateFinancialGoalAndBalance(transaction, transaction.Amount);

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

        public async Task<BaseResultModel> GetGroupTransactionDetailsAsync(Guid transactionId)
        {
            var transaction = await _unitOfWork.TransactionsRepository.GetByIdIncludeAsync(transactionId,
                query => query.Include(t => t.User).Include(t => t.Group));

            if (transaction == null)
            {
                throw new NotExistException(MessageConstants.TRANSACTION_NOT_FOUND);
            }

            var transactionModel = _mapper.Map<GroupTransactionModel>(transaction);
            if (transactionModel.Type == TransactionType.EXPENSE.ToString() && transactionModel.Status == TransactionStatus.APPROVED.ToString())
            {
                var (maxWithdrawalLimit, contributionPercentage) =
                    await CalculateWithdrawalLimitAsync(transaction.Group, transaction.UserId.Value);
                transactionModel.Note = maxWithdrawalLimit > 0
                    ? $"Hạn mức rút tối đa (khuyến nghị) của thành viên là: {maxWithdrawalLimit:N0} VND (dựa trên {contributionPercentage:F2}% đóng góp vào quỹ)"
                    : $"Thành viên chưa góp quỹ hoặc đã rút hết số tiền góp";
            }

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(transaction.Id, EntityName.TRANSACTION.ToString());
            transactionModel.Images = images.Select(i => i.ImageUrl).ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = transactionModel
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

            var pendingRequestModels = transactionModels
                .Where(t => t.ApprovalRequired && t.Status == TransactionStatus.PENDING.ToString())
                .ToList();

            if (pendingRequestModels.Any())
            {
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
            }

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

        #region Group vote

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

        #endregion

        // Helper method
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

            if (activeGoal != null && activeGoal.Status == FinancialGoalStatus.ACTIVE && activeGoal.Deadline > CommonUtils.GetCurrentTime())
            {
                // Adjust the amount based on transaction type
                decimal adjustedAmount = transaction.Type == TransactionType.INCOME ? amount : -amount;

                activeGoal.CurrentAmount += adjustedAmount;

                // Ensure CurrentAmount doesn't go below zero
                if (activeGoal.CurrentAmount < 0)
                {
                    activeGoal.CurrentAmount = 0;
                }

                if (activeGoal.CurrentAmount >= activeGoal.TargetAmount)
                {
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
    }
}
