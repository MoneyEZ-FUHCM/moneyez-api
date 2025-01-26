using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupMember;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IGroupMemberService
    {
        Task<GroupMember> AddGroupMemberAsync(CreateGroupMemberModel model);
    }
}