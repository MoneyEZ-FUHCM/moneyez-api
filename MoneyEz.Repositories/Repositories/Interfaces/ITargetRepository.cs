using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ITargetRepository : IGenericRepository<Target>
    {
        Task<Pagination<Target>> GetPaginatedTargetsAsync(int pageIndex, int pageSize);
        Task<Target?> GetTargetByIdAsync(Guid id);
    }
}
