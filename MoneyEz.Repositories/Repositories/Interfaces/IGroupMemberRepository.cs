using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Implements;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IGroupMemberRepository : IGenericRepository<GroupMember>
    {
        Task<RoleGroup> GetUserRoleInGroup(Guid userId, Guid groupId);
    }
}