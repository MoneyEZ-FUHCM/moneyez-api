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
using MoneyEz.Services.BusinessModels.TransactionModels;
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
        private readonly ITransactionService _transactionService;

        public RecurringTransactionService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService, ITransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
            _transactionService = transactionService;
        }

        public async Task<BaseResultModel> AddRecurringTransactionAsync(CreateRecurringTransactionModel model)
        {
            var user = await GetCurrentUserAsync();
            var subcategory = await GetSubcategoryAsync(model.SubcategoryId);
            var category = await GetCategoryBySubcategoryIdAsync(subcategory.Id);
            await ValidateSubcategoryInCurrentSpendingModelAsync(user.Id, model.SubcategoryId);

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

            var transactions = await _unitOfWork.RecurringTransactionRepository.GetRecurringTransactionsFilterAsync(
                paginationParameter,
                filter,
                condition: t => t.UserId == user.Id,
                include: q => q.Include(t => t.Subcategory)
            );

            var models = _mapper.Map<List<RecurringTransactionModel>>(transactions);
            
            // Calculate next occurrence date for each recurring transaction
            DateTime today = CommonUtils.GetCurrentTime().Date;
            foreach (var model in models)
            {
                model.NextOccurrence = CalculateNextOccurrence(model, today);
            }
            
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
            var transaction = await GetTransactionByIdAsync(model.Id, user.Id);
            var subcategory = await GetSubcategoryAsync(model.SubcategoryId);
            var category = await GetCategoryBySubcategoryIdAsync(subcategory.Id);
            await ValidateSubcategoryInCurrentSpendingModelAsync(user.Id, model.SubcategoryId);

            _mapper.Map(model, transaction);
            transaction.UpdatedDate = CommonUtils.GetCurrentTime();
            transaction.UpdatedBy = user.Email;
            transaction.Type = category.Type ?? throw new DefaultException("Category is missing transaction type.", MessageConstants.CATEGORY_TYPE_INVALID);

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
            var transaction = await GetTransactionByIdAsync(id, user.Id);

            _unitOfWork.RecurringTransactionRepository.SoftDeleteAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.RECURRING_TRANSACTION_DELETED_SUCCESS
            };
        }

        public async Task<BaseResultModel> GetRecurringDatesInCurrentMonthAsync()
        {
            var user = await GetCurrentUserAsync();
            DateTime today = CommonUtils.GetCurrentTime();
            var startOfMonth = new DateTime(today.Year, today.Month, 1).Date;
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).Date;

            var recurrings = await _unitOfWork.RecurringTransactionRepository.GetByConditionAsync(
                filter: rt => rt.UserId == user.Id
                    && rt.Status == CommonsStatus.ACTIVE
                    && rt.StartDate.Date <= endOfMonth
                    && (!rt.EndDate.HasValue || rt.EndDate.Value.Date >= startOfMonth)
                    && !rt.IsDeleted,
                include: q => q.Include(rt => rt.Subcategory)
            );

            HashSet<int> recurringDays = new();

            foreach (var rt in recurrings)
            {
                var dates = GetRecurringDatesInRange(rt, startOfMonth, endOfMonth);
                foreach (var date in dates)
                {
                    recurringDays.Add(date.Day);
                }
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Fetched recurring transaction days successfully.",
                Data = recurringDays.OrderBy(d => d).ToList()
            };

            //// Groups transactions by date
            //Dictionary<DateTime, List<RecurringTransaction>> dateTransactionsMap = new();
            //foreach (var rt in recurrings)
            //{
            //    var dates = GetRecurringDatesInRange(rt, startOfMonth, endOfMonth);
            //    foreach (var date in dates)
            //    {
            //        if (!dateTransactionsMap.ContainsKey(date))
            //        {
            //            dateTransactionsMap[date] = new List<RecurringTransaction>();
            //        }
            //        dateTransactionsMap[date].Add(rt);
            //    }
            //}

            //// Creates result objects with dates and their transactions
            //var result = new List<RecurringDateTransactionModel>();
            //foreach (var kvp in dateTransactionsMap.OrderBy(x => x.Key))
            //{
            //    var dateModel = new RecurringDateTransactionModel
            //    {
            //        Date = kvp.Key,
            //        Transactions = _mapper.Map<List<RecurringTransactionModel>>(kvp.Value)
            //    };
            //    result.Add(dateModel);
            //}

            //// Returns the structured data
            //return new BaseResultModel
            //{
            //    Status = StatusCodes.Status200OK,
            //    Message = "Fetched recurring transactions by date successfully.",
            //    Data = result
            //};
        }

        #region helper
        private async Task<User> GetCurrentUserAsync()
        {
            var email = _claimsService.GetCurrentUserEmail;
            return await _unitOfWork.UsersRepository.GetUserByEmailAsync(email)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);
        }

        private async Task<Subcategory> GetSubcategoryAsync(Guid subcategoryId)
        {
            return await _unitOfWork.SubcategoryRepository.GetByIdAsync(subcategoryId)
                ?? throw new NotExistException(MessageConstants.SUBCATEGORY_NOT_FOUND);
        }

        private async Task<Category> GetCategoryBySubcategoryIdAsync(Guid subcategoryId)
        {
            return await _unitOfWork.CategorySubcategoryRepository.GetCategoryBySubcategoryId(subcategoryId)
                ?? throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);
        }

        private async Task ValidateSubcategoryInCurrentSpendingModelAsync(Guid userId, Guid subcategoryId)
        {
            var currentSpendingModel = await _unitOfWork.UserSpendingModelRepository.GetCurrentSpendingModelByUserId(userId)
                ?? throw new DefaultException("Không tìm thấy UserSpendingModel đang hoạt động.", MessageConstants.USER_HAS_NO_ACTIVE_SPENDING_MODEL);

            var allowedSubcategories = await _unitOfWork.CategorySubcategoryRepository.GetSubcategoriesBySpendingModelId(currentSpendingModel.SpendingModelId.Value);
            if (!allowedSubcategories.Any(s => s.Id == subcategoryId))
            {
                throw new DefaultException("Subcategory không nằm trong SpendingModel hiện tại.", MessageConstants.SUBCATEGORY_NOT_IN_SPENDING_MODEL);
            }
        }

        private async Task<RecurringTransaction> GetTransactionByIdAsync(Guid transactionId, Guid userId)
        {
            var transaction = await _unitOfWork.RecurringTransactionRepository.GetByIdAsync(transactionId)
                ?? throw new NotExistException(MessageConstants.RECURRING_TRANSACTION_NOT_FOUND);

            if (transaction.UserId != userId)
            {
                throw new DefaultException("You cannot modify another user's recurring transaction.",
                    MessageConstants.RECURRING_TRANSACTION_ACCESS_DENIED);
            }

            return transaction;
        }
        private List<DateTime> GetRecurringDatesInRange(RecurringTransaction rt, DateTime from, DateTime to)
        {
            List<DateTime> result = new();
            DateTime current = rt.StartDate.Date > from ? rt.StartDate.Date : from;
            DateTime end = rt.EndDate.HasValue && rt.EndDate.Value.Date < to ? rt.EndDate.Value.Date : to;

            int interval = rt.Interval <= 0 ? 1 : rt.Interval;

            while (current <= end)
            {
                if (current >= from && current <= to)
                    result.Add(current.Date);

                current = rt.FrequencyType switch
                {
                    FrequencyType.DAILY => current.AddDays(interval),
                    FrequencyType.WEEKLY => current.AddDays(7 * interval),
                    FrequencyType.MONTHLY => current.AddMonths(interval),
                    FrequencyType.YEARLY => current.AddYears(interval),
                    _ => current.AddDays(interval)
                };
            }

            return result;
        }

        private DateTime? CalculateNextOccurrence(RecurringTransactionModel rt, DateTime fromDate)
        {
            // If the transaction has ended, there's no next occurrence
            if (rt.EndDate.HasValue && rt.EndDate.Value.Date < fromDate)
            {
                return null;
            }

            // If the start date is in the future, the next occurrence is the start date
            if (rt.StartDate.Date > fromDate)
            {
                return rt.StartDate.Date;
            }

            // Ensure interval is at least 1
            int interval = rt.Interval <= 0 ? 1 : rt.Interval;
            
            // Start with the start date or the current date, whichever is later
            DateTime current = rt.StartDate.Date > fromDate ? rt.StartDate.Date : fromDate;
            
            // If we're starting from the current date and it's after the start date,
            // we need to find the next occurrence based on the frequency pattern
            if (current > rt.StartDate.Date)
            {
                // Calculate how many intervals have passed
                switch (rt.FrequencyType)
                {
                    case FrequencyType.DAILY:
                        // If current date is a due date, move to the next one
                        if ((current - rt.StartDate.Date).Days % interval == 0)
                        {
                            current = current.AddDays(interval);
                        }
                        else
                        {
                            // Find the next occurrence
                            int daysElapsed = (current - rt.StartDate.Date).Days;
                            int daysToAdd = interval - (daysElapsed % interval);
                            current = current.AddDays(daysToAdd);
                        }
                        break;
                        
                    case FrequencyType.WEEKLY:
                        int weeksElapsed = (current - rt.StartDate.Date).Days / 7;
                        if (weeksElapsed % interval == 0)
                        {
                            // If today is the occurrence day, move to next interval
                            current = current.AddDays(7 * interval);
                        }
                        else
                        {
                            // Find the next weekly occurrence
                            int weeksToAdd = interval - (weeksElapsed % interval);
                            current = current.AddDays(7 * weeksToAdd);
                        }
                        break;
                        
                    case FrequencyType.MONTHLY:
                        int monthsElapsed = MonthsBetween(rt.StartDate.Date, current);
                        if (monthsElapsed % interval == 0 && current.Day == rt.StartDate.Day)
                        {
                            // If today is the occurrence day (same day of month), move to next interval
                            current = current.AddMonths(interval);
                        }
                        else
                        {
                            // Find the next monthly occurrence
                            int monthsToAdd = interval - (monthsElapsed % interval);
                            // Try to maintain the same day of month
                            DateTime nextDate = current.AddMonths(monthsToAdd);
                            // Adjust if the day doesn't exist in the target month
                            int targetDay = Math.Min(rt.StartDate.Day, DateTime.DaysInMonth(nextDate.Year, nextDate.Month));
                            current = new DateTime(nextDate.Year, nextDate.Month, targetDay);
                        }
                        break;
                        
                    case FrequencyType.YEARLY:
                        int yearsElapsed = current.Year - rt.StartDate.Year;
                        if (yearsElapsed % interval == 0 && current.Month == rt.StartDate.Month && current.Day == rt.StartDate.Day)
                        {
                            // If today is the anniversary, move to next interval
                            current = current.AddYears(interval);
                        }
                        else
                        {
                            // Find the next yearly occurrence
                            int yearsToAdd = interval - (yearsElapsed % interval);
                            if (yearsElapsed % interval != 0 || 
                                (current.Month < rt.StartDate.Month) || 
                                (current.Month == rt.StartDate.Month && current.Day < rt.StartDate.Day))
                            {
                                // We haven't reached this year's occurrence yet
                                yearsToAdd = yearsElapsed % interval == 0 ? 0 : yearsToAdd;
                            }
                            
                            // Try to maintain same month and day
                            int targetYear = current.Year + yearsToAdd;
                            int targetMonth = rt.StartDate.Month;
                            int targetDay = Math.Min(rt.StartDate.Day, DateTime.DaysInMonth(targetYear, targetMonth));
                            current = new DateTime(targetYear, targetMonth, targetDay);
                        }
                        break;
                        
                    default:
                        current = current.AddDays(interval);
                        break;
                }
            }
            else
            {
                // If we're starting from the start date, just add one interval
                current = rt.FrequencyType switch
                {
                    FrequencyType.DAILY => current.AddDays(interval),
                    FrequencyType.WEEKLY => current.AddDays(7 * interval),
                    FrequencyType.MONTHLY => current.AddMonths(interval),
                    FrequencyType.YEARLY => current.AddYears(interval),
                    _ => current.AddDays(interval)
                };
            }
            
            // Check if the calculated next occurrence is after the end date
            if (rt.EndDate.HasValue && current > rt.EndDate.Value.Date)
            {
                return null; // No more occurrences
            }
            
            return current;
        }

        private int MonthsBetween(DateTime from, DateTime to)
        {
            return (to.Year - from.Year) * 12 + (to.Month - from.Month);
        }

        #endregion helper

        #region job

        /*
            today == StartDate + n × interval (theo frequency)

            today >= StartDate

            today <= EndDate (nếu có)
         
         */

        public async Task GenerateTransactionsFromRecurringAsync()
        {
            DateTime today = CommonUtils.GetCurrentTime().Date;

            var recurrings = await _unitOfWork.RecurringTransactionRepository.GetByConditionAsync(
                filter: rt => rt.Status == CommonsStatus.ACTIVE
                    && rt.StartDate.Date <= today
                    && (!rt.EndDate.HasValue || rt.EndDate.Value.Date >= today),
                include: q => q.Include(r => r.Subcategory)
            );

            foreach (var rt in recurrings)
            {
                if (!IsRecurringDueToday(rt, today)) continue;

                var existing = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                    filter: t => t.InsertType == InsertType.RECURRENCE &&
                                 t.TransactionDate.HasValue && t.TransactionDate.Value.Date == today &&
                                 t.UserId.HasValue && t.UserId.Value == rt.UserId &&
                                 t.SubcategoryId.HasValue && t.SubcategoryId.Value == rt.SubcategoryId &&
                                 t.Description == $"[Recurring] {rt.Description}" &&
                                 t.Amount == rt.Amount
                );

                if (existing.Any()) continue;

                // get user onwer
                var user = await _unitOfWork.UsersRepository.GetByIdAsync(rt.UserId);

                if (user == null) continue;

                var newTransaction = new CreateTransactionModel
                {
                    Amount = rt.Amount,
                    Description = $"[Recurring] {rt.Description}",
                    TransactionDate = today,
                    SubcategoryId = rt.SubcategoryId,
                    InsertType = InsertType.RECURRENCE
                };

                await _transactionService.CreateTransactionAsync(newTransaction, user.Email);

                //var transaction = new Transaction
                //{
                //    Amount = rt.Amount,
                //    Description = $"[Recurring] {rt.Description}",
                //    TransactionDate = today,
                //    Status = TransactionStatus.APPROVED,
                //    Type = rt.Type,
                //    SubcategoryId = rt.SubcategoryId,
                //    UserId = rt.UserId,
                //    CreatedDate = today,
                //    CreatedBy = "RecurringJob",
                //    ApprovalRequired = false,
                //    InsertType = InsertType.RECURRENCE
                //};

                //await _unitOfWork.TransactionsRepository.AddAsync(transaction);
            }
        }

        private bool IsRecurringDueToday(RecurringTransaction rt, DateTime today)
        {
            var start = rt.StartDate.Date;
            if (today < start) return false;
            if (rt.EndDate.HasValue && today > rt.EndDate.Value.Date) return false;

            int interval = rt.Interval <= 0 ? 1 : rt.Interval;

            return rt.FrequencyType switch
            {
                FrequencyType.DAILY => (today - start).Days % interval == 0,

                FrequencyType.WEEKLY => ((today - start).Days / 7) % interval == 0,

                FrequencyType.MONTHLY => MonthsBetween(start, today) % interval == 0,

                FrequencyType.YEARLY => (today.Year - start.Year) % interval == 0,

                _ => false
            };
        }

        #endregion job
    }
}
