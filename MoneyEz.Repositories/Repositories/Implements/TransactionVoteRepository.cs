using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class TransactionVoteRepository : GenericRepository<TransactionVote>, ITransactionVoteRepository
    {
        private readonly MoneyEzContext _context;

        public TransactionVoteRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<TransactionVote>> GetVotesByTransactionIdAsync(Guid transactionId)
        {
            return await _context.Set<TransactionVote>()
                .Where(v => v.TransactionId == transactionId)
                .ToListAsync();
        }
    }
}
