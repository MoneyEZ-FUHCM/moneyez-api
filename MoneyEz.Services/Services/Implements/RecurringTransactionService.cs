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

            DateTime today = CommonUtils.GetCurrentTime().Date;
            var model = _mapper.Map<RecurringTransactionModel>(transaction);
            model.NextOccurrence = CalculateNextOccurrence(model, today);
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

        private static DateTime? CalculateNextOccurrence(RecurringTransactionModel rt, DateTime fromDate)
        {
            // Nếu giao dịch đã kết thúc, không có lần tiếp theo
            if (rt.EndDate.HasValue && rt.EndDate.Value.Date < fromDate.Date)
            {
                return null;
            }

            // Nếu ngày bắt đầu ở tương lai, lần tiếp theo là ngày bắt đầu
            if (rt.StartDate.Date > fromDate.Date)
            {
                return rt.StartDate.Date;
            }

            // Đảm bảo interval ít nhất là 1
            int interval = Math.Max(1, rt.Interval);

            // Tính toán dựa trên loại tần suất
            DateTime nextOccurrence;

            switch (rt.FrequencyType)
            {
                case FrequencyType.DAILY:
                    // Tính số ngày kể từ ngày bắt đầu
                    int daysSinceStart = (fromDate.Date - rt.StartDate.Date).Days;

                    // Tính ngày tiếp theo
                    if (daysSinceStart % interval == 0)
                    {
                        // Nếu hôm nay là ngày xảy ra, lấy ngày tiếp theo
                        nextOccurrence = fromDate.Date.AddDays(interval);
                    }
                    else
                    {
                        // Tìm ngày tiếp theo dựa trên mẫu
                        int daysRemaining = interval - (daysSinceStart % interval);
                        nextOccurrence = fromDate.Date.AddDays(daysRemaining);
                    }
                    break;

                case FrequencyType.WEEKLY:

                    // Nếu ngày bắt đầu chưa qua
                    if (rt.StartDate.Date > fromDate.Date)
                    {
                        return rt.StartDate.Date;
                    }

                    // Nếu fromDate là ngày bắt đầu hoặc trước ngày bắt đầu, lần đầu tiên là ngày bắt đầu
                    if (fromDate.Date <= rt.StartDate.Date)
                    {
                        nextOccurrence = rt.StartDate.Date;
                    }
                    else
                    {
                        // Tính số ngày từ ngày bắt đầu
                        int daysDiff = (fromDate.Date - rt.StartDate.Date).Days;

                        // Tính số tuần đã trôi qua
                        int weeksPassed = daysDiff / 7;

                        // Tính số lần xảy ra đã qua (dựa vào chu kỳ interval)
                        int occurrencesPassed = weeksPassed / interval;

                        // Tính ngày của lần xảy ra gần nhất
                        DateTime lastOccurrence = rt.StartDate.Date.AddDays(occurrencesPassed * interval * 7);

                        // Nếu ngày hiện tại chưa qua ngày này
                        if (fromDate.Date < lastOccurrence)
                        {
                            nextOccurrence = lastOccurrence;
                        }
                        else
                        {
                            // Lần xảy ra tiếp theo
                            nextOccurrence = lastOccurrence.Date.AddDays(interval * 7);
                        }
                    }
            
                    break;

                case FrequencyType.MONTHLY:
                    // Tính số tháng từ ngày bắt đầu
                    int monthsSinceStart = (fromDate.Year - rt.StartDate.Year) * 12 + (fromDate.Month - rt.StartDate.Month);

                    // Ngày trong tháng (điều chỉnh cho tháng ngắn hơn)
                    int dayOfMonthToUse = Math.Min(rt.StartDate.Day, DateTime.DaysInMonth(fromDate.Year, fromDate.Month));

                    if (monthsSinceStart % interval == 0 && fromDate.Day == dayOfMonthToUse)
                    {
                        // Hôm nay là ngày xảy ra, chuyển đến chu kỳ tiếp theo
                        nextOccurrence = fromDate.Date.AddMonths(interval);
                    }
                    else
                    {
                        if (fromDate.Day < dayOfMonthToUse && monthsSinceStart % interval == 0)
                        {
                            // Nếu chúng ta chưa vượt qua ngày trong tháng này và tháng này phù hợp với mẫu
                            nextOccurrence = new DateTime(fromDate.Year, fromDate.Month, dayOfMonthToUse);
                        }
                        else
                        {
                            // Tính số tháng cần thêm vào
                            int monthsToAdd = interval - (monthsSinceStart % interval);
                            if (monthsSinceStart % interval == 0) monthsToAdd = interval;

                            // Tính ngày tiếp theo
                            DateTime nextMonth = new DateTime(fromDate.Year, fromDate.Month, 1).AddMonths(monthsToAdd);
                            int daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                            int nextMonthDay = Math.Min(rt.StartDate.Day, daysInNextMonth);

                            nextOccurrence = new DateTime(nextMonth.Year, nextMonth.Month, nextMonthDay);
                        }
                    }
                    break;

                case FrequencyType.YEARLY:
                    // Tính số năm từ ngày bắt đầu
                    int yearsSinceStart = fromDate.Year - rt.StartDate.Year;

                    // Ngày trong năm nay phù hợp với mẫu
                    int daysInMonthThisYear = DateTime.DaysInMonth(fromDate.Year, rt.StartDate.Month);
                    int dayToUse = Math.Min(rt.StartDate.Day, daysInMonthThisYear);

                    DateTime thisYearDate = new DateTime(fromDate.Year, rt.StartDate.Month, dayToUse);

                    if (yearsSinceStart % interval == 0 && fromDate.Date <= thisYearDate.Date)
                    {
                        // Năm nay phù hợp và chúng ta chưa vượt qua ngày
                        nextOccurrence = thisYearDate;
                    }
                    else
                    {
                        // Tính năm tiếp theo phù hợp với mẫu
                        int yearsToAdd = interval - (yearsSinceStart % interval);
                        if (yearsSinceStart % interval == 0) yearsToAdd = interval;

                        int nextYear = fromDate.Year + yearsToAdd;
                        int daysInMonth = DateTime.DaysInMonth(nextYear, rt.StartDate.Month);
                        int nextDay = Math.Min(rt.StartDate.Day, daysInMonth);

                        nextOccurrence = new DateTime(nextYear, rt.StartDate.Month, nextDay);
                    }
                    break;

                default:
                    // Mặc định nếu có lỗi
                    nextOccurrence = fromDate.Date.AddDays(interval);
                    break;
            }

            // Kiểm tra xem ngày tiếp theo có vượt quá ngày kết thúc không
            if (rt.EndDate.HasValue && nextOccurrence.Date > rt.EndDate.Value.Date)
            {
                return null; // Không còn lần tiếp theo
            }

            return nextOccurrence;
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
                    && !rt.IsDeleted
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
