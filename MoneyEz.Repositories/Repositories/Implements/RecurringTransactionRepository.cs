using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class RecurringTransactionRepository : GenericRepository<RecurringTransaction>, IRecurringTransactionRepository
    {
        private readonly MoneyEzContext _context;

        public RecurringTransactionRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }


        public async Task<Pagination<RecurringTransaction>> GetRecurringTransactionsFilterAsync(
            PaginationParameter paginationParameter,
            RecurringTransactionFilter filter,
            Expression<Func<RecurringTransaction, bool>>? condition = null,
            Func<IQueryable<RecurringTransaction>, IIncludableQueryable<RecurringTransaction, object>>? include = null)
        {
            var query = _context.RecurringTransactions.AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            if (condition != null)
            {
                query = query.Where(condition);
            }

            query = ApplyRecurringTransactionFiltering(query, filter);

            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                   .Take(paginationParameter.PageSize)
                                   .AsNoTracking()
                                   .ToListAsync();

            return new Pagination<RecurringTransaction>(items, itemCount, paginationParameter.PageIndex, paginationParameter.PageSize);
        }

        private IQueryable<RecurringTransaction> ApplyRecurringTransactionFiltering(IQueryable<RecurringTransaction> query, RecurringTransactionFilter filter)
        {
            if (filter == null) return query;

            query = query.Where(x => x.IsDeleted == filter.IsDeleted);

            if (filter.SubcategoryId.HasValue)
                query = query.Where(t => t.SubcategoryId == filter.SubcategoryId);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.StartDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.StartDate <= filter.ToDate.Value);

            if (filter.IsActive.HasValue)
            {
                query = query.Where(t => filter.IsActive.Value
                    ? t.Status == CommonsStatus.ACTIVE
                    : t.Status != CommonsStatus.ACTIVE);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var keyword = filter.Search.ToLower();
                query = query.Where(t =>
                    (!string.IsNullOrEmpty(t.Description) && t.Description.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(t.Tags) && t.Tags.ToLower().Contains(keyword)));
            }

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                bool isAsc = string.IsNullOrEmpty(filter.Dir) || filter.Dir.ToLower() == "asc";
                query = filter.SortBy.ToLower() switch
                {
                    "amount" => isAsc ? query.OrderBy(t => t.Amount) : query.OrderByDescending(t => t.Amount),
                    "startdate" => isAsc ? query.OrderBy(t => t.StartDate) : query.OrderByDescending(t => t.StartDate),
                    "description" => isAsc ? query.OrderBy(t => t.Description) : query.OrderByDescending(t => t.Description),
                    _ => query.OrderByDescending(t => t.StartDate)
                };
            }
            else
            {
                query = query.OrderByDescending(t => t.StartDate);
            }

            return query;
        }


    }
}
