using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.GroupFund;
using MoneyEz.Services.BusinessModels.GroupFund.GroupInvite;
using MoneyEz.Services.BusinessModels.GroupMember;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IGroupFundsService
    {
        Task<BaseResultModel> CreateGroupFundsAsync(CreateGroupModel model);
        Task<BaseResultModel> GetAllGroupFunds(PaginationParameter paginationParameters);
        Task<BaseResultModel> CloseGroupFundAsync(Guid groupId);
        Task<BaseResultModel> RemoveMemberByLeaderAsync(Guid groupId, Guid memberId);
        Task<BaseResultModel> SetMemberRoleAsync(SetRoleGroupModel setRoleGroupModel);
        Task<BaseResultModel> GenerateFinancialHealthReportAsync(Guid groupId);
        Task<BaseResultModel> InviteMemberEmailAsync(InviteMemberModel inviteMemberModel);
        Task<BaseResultModel> InviteMemberQRCodeAsync(InviteMemberModel inviteMemberModel);
        Task<BaseResultModel> AcceptInvitationEmailAsync(string token);
        Task<BaseResultModel> AcceptInvitationQRCodeAsync(string token);
        Task<BaseResultModel> GetGroupFundById(Guid groupId);
        Task<BaseResultModel> LeaveGroupAsync(Guid groupId);
        Task<BaseResultModel> SetGroupContribution(SetGroupContributionModel setGroupContributionModel);

    }
}