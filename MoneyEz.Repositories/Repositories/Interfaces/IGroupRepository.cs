using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IGroupRepository : IGenericRepository<GroupFund>
    {
        // Add any additional methods specific to GroupRepository if needed
        Task<GroupFund> CreateGroupFundAsync(GroupFund groupFund);
    }
}
