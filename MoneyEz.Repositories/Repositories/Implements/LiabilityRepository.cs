using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class LiabilityRepository : GenericRepository<Liability>, ILiabilityRepository
    {
        public LiabilityRepository(MoneyEzContext context) : base(context){}
    }
}
