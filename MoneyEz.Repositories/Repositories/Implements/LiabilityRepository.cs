using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class LiabilityRepository : GenericRepository<Liability>, ILiabilityRepository
    {
        public LiabilityRepository(MoneyEzContext context) : base(context) { }
        public async Task<Pagination<Liability>> GetLiabilitiesByUserIdAsync(Guid userId, PaginationParameter paginationParameter)
        {
            return await ToPaginationIncludeAsync(
                paginationParameter,
                filter: Liability => Liability.UserId == userId && !Liability.IsDeleted
            );
        }
    }
}
