using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
    {
        private readonly MoneyEzContext _context;

        public SubscriptionRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }
    }
}
