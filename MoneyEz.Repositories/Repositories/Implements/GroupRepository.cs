using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class GroupRepository : GenericRepository<GroupFund>, IGroupFundRepository
    {
        private readonly MoneyEzContext _context;

        public GroupRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<GroupFund>> GetByAccountBankId(Guid accountBankId)
        {
            return _context.GroupFunds.Where(x => x.AccountBankId == accountBankId).ToListAsync();
        }
    }
}
