using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IGroupFundRepository : IGenericRepository<GroupFund>
    {
        public Task<List<GroupFund>> GetByAccountBankId(Guid accountBankId);
    }
}
