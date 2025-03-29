using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class RecurringTransactionRepository : GenericRepository<RecurringTransaction>, IRecurringTransactionRepository
    {
        private readonly MoneyEzContext _context;

        public RecurringTransactionRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

    }
}
