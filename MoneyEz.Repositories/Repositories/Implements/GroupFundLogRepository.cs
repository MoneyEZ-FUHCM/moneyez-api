using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class GroupFundLogRepository : GenericRepository<GroupFundLog>, IGroupFundLogRepository
    {
        private readonly MoneyEzContext _context;
        public GroupFundLogRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pagination<GroupFundLog>> GetGroupFundLogsFilter(PaginationParameter paginationParameters, GroupLogFilter filter,
            Expression<Func<GroupFundLog, bool>>? condition = null)
        {
            var query = _context.GroupFundLogs.AsQueryable();

            if (condition != null)
            {
                query = query.Where(condition);
            }

            // Apply additional filters
            query = ApplyGroupFundLogFiltering(query, filter);
            
            var itemCount = await query.CountAsync();
            var items = await query.Skip((paginationParameters.PageIndex - 1) * paginationParameters.PageSize)
                                   .Take(paginationParameters.PageSize)
                                   .AsNoTracking()
                                   .ToListAsync();
                                   
            var result = new Pagination<GroupFundLog>(items, itemCount, paginationParameters.PageIndex, paginationParameters.PageSize);
            return result;
        }
        
        private IQueryable<GroupFundLog> ApplyGroupFundLogFiltering(IQueryable<GroupFundLog> query, GroupLogFilter filter)
        {
            if (filter == null) return query;
            
            // Apply IsDeleted filter from FilterBase
            query = query.Where(g => g.IsDeleted == filter.IsDeleted);
            
            // Apply ChangeType filter if provided
            if (!string.IsNullOrEmpty(filter.ChangeType))
            {
                query = filter.ChangeType.ToLower() switch
                {
                    "group" => query.Where(g =>
                                                g.Action == GroupAction.CREATED.ToString() ||
                                                g.Action == GroupAction.UPDATED.ToString() ||
                                                g.Action == GroupAction.DISBANDED.ToString() ||
                                                g.Action == GroupAction.TRANSACTION_CREATED.ToString() ||
                                                g.Action == GroupAction.TRANSACTION_UPDATED.ToString() ||
                                                g.Action == GroupAction.TRANSACTION_DELETED.ToString()),

                    "member" => query.Where(g =>
                                                g.Action == GroupAction.INVITED.ToString() ||
                                                g.Action == GroupAction.JOINED.ToString() ||
                                                g.Action == GroupAction.LEFT.ToString() ||
                                                g.Action == GroupAction.KICKED.ToString()),

                    _ => query.Where(g => g.Action == filter.ChangeType),
                };
            }
            
            // Apply date range filters
            if (filter.FromDate.HasValue)
            {
                query = query.Where(g => g.CreatedDate >= filter.FromDate.Value);
            }
            
            if (filter.ToDate.HasValue)
            {
                query = query.Where(g => g.CreatedDate <= filter.ToDate.Value);
            }
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(g => 
                    g.ChangeDescription != null && g.ChangeDescription.Contains(filter.Search) ||
                    g.ChangedBy != null && g.ChangedBy.Contains(filter.Search) ||
                    g.Action != null && g.Action.Contains(filter.Search)
                );
            }
            
            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                bool isAscending = string.IsNullOrEmpty(filter.Dir) || filter.Dir.ToLower() == "asc";
                
                query = filter.SortBy.ToLower() switch
                {
                    "action" => isAscending ? query.OrderBy(g => g.Action) : query.OrderByDescending(g => g.Action),
                    "changedby" => isAscending ? query.OrderBy(g => g.ChangedBy) : query.OrderByDescending(g => g.ChangedBy),
                    "date" => isAscending ? query.OrderBy(g => g.CreatedDate) : query.OrderByDescending(g => g.CreatedDate),
                    _ => query.OrderByDescending(g => g.CreatedDate) // Default sort by created date desc
                };
            }
            else
            {
                // Default sorting by created date descending if no sort specified
                query = query.OrderByDescending(g => g.CreatedDate);
            }
            
            return query;
        }
    }
}