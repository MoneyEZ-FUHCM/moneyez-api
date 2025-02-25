using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IAssetRepository : IGenericRepository<Asset>
    {
        Task<Pagination<Asset>> GetAssetsByUserIdAsync(Guid userId, PaginationParameter paginationParameter);
    }
}
