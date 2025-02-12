using MoneyEz.Repositories.Entities;

using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class AssetRepository : GenericRepository<Asset>, IAssetRepository
    {
        public AssetRepository(MoneyEzContext context) : base(context) { }
        public async Task<Pagination<Asset>> GetAssetsByUserIdAsync(Guid userId, PaginationParameter paginationParameter)
        {
            return await ToPaginationIncludeAsync(
                paginationParameter,
                filter: asset => asset.UserId == userId && !asset.IsDeleted
            );
        }
    }
}
