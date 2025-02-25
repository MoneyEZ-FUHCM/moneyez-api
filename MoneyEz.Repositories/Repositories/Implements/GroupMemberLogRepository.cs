using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class GroupMemberLogRepository : GenericRepository<GroupMemberLog>, IGroupMemberLogRepository
    {
        public GroupMemberLogRepository(MoneyEzContext context) : base(context)
        {
        }
    }
}
