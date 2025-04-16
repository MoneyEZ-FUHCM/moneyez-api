using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        private readonly MoneyEzContext _context;

        public TransactionRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalIncomeAsync(Guid? userId, Guid? groupId, DateTime startDate, DateTime endDate)
        {
            var query = _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Type == TransactionType.INCOME);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            if (groupId.HasValue)
                query = query.Where(t => t.GroupId == groupId.Value);
            else
                query = query.Where(t => t.GroupId == null);

            return await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
        }

        public async Task<decimal> GetTotalExpenseAsync(Guid? userId, Guid? groupId, DateTime startDate, DateTime endDate)
        {
            var query = _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Type == TransactionType.EXPENSE);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            if (groupId.HasValue)
                query = query.Where(t => t.GroupId == groupId.Value);
            else
                query = query.Where(t => t.GroupId == null);

            return await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
        }


        public async Task<decimal> GetTotalExpenseByCategory(Guid userId, Guid categoryId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId &&
                            t.TransactionDate >= startDate && t.TransactionDate <= endDate &&
                            t.Type == TransactionType.EXPENSE &&
                            t.Subcategory.CategorySubcategories.Any(cs => cs.CategoryId == categoryId))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
        }


        public async Task<decimal> GetTotalIncomeByCategory(Guid userId, Guid categoryId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId &&
                            t.TransactionDate >= startDate && t.TransactionDate <= endDate &&
                            t.Type == TransactionType.INCOME &&
                            t.Subcategory.CategorySubcategories.Any(cs => cs.CategoryId == categoryId))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
        }

        public async Task<Pagination<Transaction>> GetTransactionsFilterAsync(PaginationParameter paginationParameter, 
                        TransactionFilter transactionFilter,
                        Expression<Func<Transaction, bool>>? condition = null,
                        Func<IQueryable<Transaction>, IIncludableQueryable<Transaction, object>>? include = null)
        {
            var query = _context.Transactions.AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            if (condition != null)
            {
                query = query.Where(condition);
            }

            // apply filter
            query = ApplyTransactionFiltering(query, transactionFilter);

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                    .Take(paginationParameter.PageSize)
                                    .AsNoTracking()
                                    .ToListAsync();
            var result = new Pagination<Transaction>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
            return result;
        }

        private IQueryable<Transaction> ApplyTransactionFiltering(IQueryable<Transaction> query, TransactionFilter filter)
        {
            if (filter == null) return query;

            // Apply IsDeleted filter
            query = query.Where(u => u.IsDeleted == filter.IsDeleted);

            if (filter.SubcategoryId.HasValue)
            {
                query = query.Where(t => t.SubcategoryId == filter.SubcategoryId.Value);
            }

            if (filter.Type.HasValue)
            {
                query = query.Where(t => t.Type == filter.Type.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= filter.ToDate.Value);
            }

            // filter by status
            if (!string.IsNullOrEmpty(filter.Status))
            {
                switch (filter.Status.ToLower())
                {
                    case "confirmed":
                        query = query.Where(t => t.Status == TransactionStatus.APPROVED || t.Status == TransactionStatus.REJECTED);
                        break;
                    case "approved":
                        query = query.Where(t => t.Status == TransactionStatus.APPROVED);
                        break;
                    case "rejected":
                        query = query.Where(t => t.Status == TransactionStatus.REJECTED);
                        break;
                    case "pending":
                        query = query.Where(t => t.Status == TransactionStatus.PENDING);
                        break;
                }
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var isAscending = string.IsNullOrEmpty(filter.Dir) || filter.Dir.ToLower() == "asc";

                query = filter.SortBy.ToLower() switch
                {
                    "amount" => isAscending ? query.OrderBy(t => t.Amount) : query.OrderByDescending(t => t.Amount),
                    "transaction_date" => isAscending ? query.OrderBy(t => t.TransactionDate) : query.OrderByDescending(t => t.TransactionDate),
                    "created_date" => isAscending ? query.OrderBy(t => t.CreatedDate) : query.OrderByDescending(t => t.CreatedDate),
                    _ => query.OrderByDescending(t => t.TransactionDate) // Default sort by date desc
                };
            }
            else
            {
                // Default sorting by transaction date descending if no sort specified
                query = query.OrderByDescending(t => t.TransactionDate);
            }

            return query;
        }

        public async Task<decimal> GetToalIncomeByUserSpendingModelAsync(Guid userSpendingModelId)
        {
            var query = _context.Transactions
                .Where(t => t.UserSpendingModelId == userSpendingModelId && t.Type == TransactionType.INCOME && t.IsDeleted == false);

            return await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
        }


        public IQueryable<Transaction> FilterByType(IQueryable<Transaction> source, ReportTransactionType type)
        {
            if (type == ReportTransactionType.Total)
            {
                return source;
            }

            var desiredType = type == ReportTransactionType.Income ? TransactionType.INCOME : TransactionType.EXPENSE;

            return source.Where(t =>
                t.Subcategory != null &&
                t.Subcategory.CategorySubcategories.Any(cs =>
                    cs.Category != null && cs.Category.Type == desiredType
                ));
        }

    }
}