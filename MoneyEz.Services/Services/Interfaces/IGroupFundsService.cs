using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupFund;
using MoneyEz.Services.BusinessModels.GroupMember;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IGroupFundsService
    {
        Task<BaseResultModel> CreateGroupFundsAsync(CreateGroupModel model);
        Task<BaseResultModel> GetAllGroupFunds();
        Task<BaseResultModel> DisbandGroupAsync(Guid groupId);

    }
}