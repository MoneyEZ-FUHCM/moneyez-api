using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System.Threading.Tasks;
using MoneyEz.Repositories.Enums;
using Microsoft.EntityFrameworkCore;


namespace MoneyEz.Repositories.Repositories.Implements
{
    public class GroupMemberRepository : GenericRepository<GroupMember>, IGroupMemberRepository
    {
        private readonly MoneyEzContext _context;

        public GroupMemberRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddAsync(GroupMember entity)
        {
            await _context.GroupMembers.AddAsync(entity);
        }

        public async Task<RoleGroup> GetUserRoleInGroup(Guid userId, Guid groupId)
        {
            var member = await _context.Set<GroupMember>()
                .Where(gm => gm.UserId == userId && gm.GroupId == groupId)
                .Select(gm => gm.Role)
                .FirstOrDefaultAsync();

            return member ?? RoleGroup.MEMBER;
        }
    }
}