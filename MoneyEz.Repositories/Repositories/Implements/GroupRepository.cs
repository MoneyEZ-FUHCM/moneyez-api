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
    public class GroupRepository : GenericRepository<GroupFund>, IGroupRepository
    {
        private readonly MoneyEzContext _context;

        public GroupRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

    }
}
