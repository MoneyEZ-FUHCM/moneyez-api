using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Implements;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IGroupFundLogRepository : IGenericRepository<GroupFundLog>
    {
        Task<Pagination<GroupFundLog>> GetGroupFundLogsFilter(PaginationParameter paginationParameters, GroupLogFilter filter,
            Expression<Func<GroupFundLog, bool>>? condition = null);
    }
}