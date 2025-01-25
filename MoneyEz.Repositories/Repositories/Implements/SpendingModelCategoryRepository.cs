using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class SpendingModelCategoryRepository : GenericRepository<SpendingModelCategory>, ISpendingModelCategoryRepository
    {
        public SpendingModelCategoryRepository(MoneyEzContext context) : base(context)
        {
        }
    }
}
