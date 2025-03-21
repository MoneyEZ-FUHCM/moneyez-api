using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using MoneyEz.Repositories.Utils;

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
                            && usm.StartDate <= CommonUtils.GetCurrentTime()
                            && usm.EndDate > CommonUtils.GetCurrentTime()
                            && usm.Status == UserSpendingModelStatus.ACTIVE
                            && !usm.IsDeleted)
                .OrderByDescending(usm => usm.StartDate)
                .FirstOrDefaultAsync();
        }
    }
}