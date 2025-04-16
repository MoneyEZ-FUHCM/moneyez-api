using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<Pagination<Post>> GetPostsByFilter(PaginationParameter paginationParameter, PostFilter filter);
    }
}
