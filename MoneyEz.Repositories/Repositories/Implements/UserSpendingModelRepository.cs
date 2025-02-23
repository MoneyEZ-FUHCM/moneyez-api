using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class UserSpendingModelRepository : GenericRepository<UserSpendingModel>, IUserSpendingModelRepository
    {
        public UserSpendingModelRepository(MoneyEzContext context) : base(context)
        {
        }
    }
}
