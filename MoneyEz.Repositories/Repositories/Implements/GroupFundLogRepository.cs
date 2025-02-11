using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class GroupFundLogRepository : GenericRepository<GroupFundLog>, IGroupFundLogRepository
    {
        private readonly MoneyEzContext _context;
        public GroupFundLogRepository(MoneyEzContext context) : base(context)
        {
        }
    }
}