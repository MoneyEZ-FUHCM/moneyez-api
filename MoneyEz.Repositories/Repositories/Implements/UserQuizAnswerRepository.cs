using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class UserQuizAnswerRepository : GenericRepository<UserQuizAnswer>, IUserQuizAnswerRepository
    {
        private readonly MoneyEzContext _context;

        public UserQuizAnswerRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

    }
}
