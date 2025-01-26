using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class SpendingModelRepository : GenericRepository<SpendingModel>, ISpendingModelRepository
    {
        public SpendingModelRepository(MoneyEzContext context) : base(context) { }
    }
}
