using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class FinancialReportRepository : GenericRepository<FinancialReport>, IFinancialReportRepository
    {
        private readonly MoneyEzContext _context;

        public FinancialReportRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }
    }
}
