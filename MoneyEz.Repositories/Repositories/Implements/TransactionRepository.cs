using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using Org.BouncyCastle.Asn1;
using System;
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

        public async Task<decimal> GetTotalIncomeAsync(Guid? userId, Guid? groupId, DateTime startDate, DateTime endDate)
        {
            var query = _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Type == TransactionType.INCOME);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            if (groupId.HasValue)
                query = query.Where(t => t.GroupId == groupId.Value);

            return await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
        }

        public async Task<decimal> GetTotalExpenseAsync(Guid? userId, Guid? groupId, DateTime startDate, DateTime endDate)
        {
            var query = _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.Type == TransactionType.EXPENSE);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            if (groupId.HasValue)
                query = query.Where(t => t.GroupId == groupId.Value);

            return await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
        }


        public async Task<decimal> GetTotalExpenseByCategory(Guid userId, Guid categoryId, DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId &&
                            t.TransactionDate >= startDate && t.TransactionDate <= endDate &&
                            t.Type == TransactionType.EXPENSE &&
                            t.Subcategory.CategorySubcategories.Any(cs => cs.CategoryId == categoryId))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
        }

    }
}