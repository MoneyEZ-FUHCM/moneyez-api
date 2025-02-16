﻿using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly MoneyEzContext _context;

        public UserRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetUserByPhoneAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.PhoneNumber == phoneNumber);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<List<User>> GetUsersByUserIdsAsync(List<Guid> userIds)
        {
            return await _context.Users.Where(x => userIds.Contains(x.Id)).ToListAsync();
        }
    }
}
