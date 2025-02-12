using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ILiabilityRepository : IGenericRepository<Liability>
    {
        Task<Pagination<Liability>> GetLiabilitiesByUserIdAsync(Guid userId, PaginationParameter paginationParameter);
    }
}
