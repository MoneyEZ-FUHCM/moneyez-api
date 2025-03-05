using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class UserSpendingModelRepository : GenericRepository<UserSpendingModel>, IUserSpendingModelRepository
    {
        private readonly MoneyEzContext _context;

        public UserSpendingModelRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UserSpendingModel?> GetCurrentSpendingModelByUserId(Guid userId)
        {
            return await _context.UserSpendingModels
                .Where(usm => usm.UserId == userId
                            && usm.StartDate <= DateTime.UtcNow
                            && usm.EndDate >= DateTime.UtcNow
                            && !usm.IsDeleted)
                .OrderByDescending(usm => usm.StartDate)
                .FirstOrDefaultAsync();
        }
    }
}