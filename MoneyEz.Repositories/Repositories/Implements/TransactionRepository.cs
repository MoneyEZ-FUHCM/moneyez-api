
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        private readonly MoneyEzContext _context;

        public TransactionRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

    }
}