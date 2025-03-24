using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ITransactionVoteRepository : IGenericRepository<TransactionVote>
    {
        Task<List<TransactionVote>> GetVotesByTransactionIdAsync(Guid transactionId);
    }
}
