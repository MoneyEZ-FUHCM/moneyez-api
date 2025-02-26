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
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetUserByEmailAsync(string email);

        Task<User> GetUserByPhoneAsync(string phoneNumber);

        Task<List<User>> GetUsersByUserIdsAsync(List<Guid> userIds);

        Task<Pagination<User>> GetUsersByFilter(PaginationParameter paginationParameter, UserFilter filter);

    }
}
