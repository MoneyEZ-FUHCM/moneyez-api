using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.GroupMember;
using System;
using System.Threading.Tasks;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.GroupFund;
using Microsoft.AspNetCore.Authorization;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.GroupFund.GroupInvite;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/groups")]
    [ApiController]
    public class GroupController : BaseController
    {
        private readonly IGroupFundsService _groupFundsService;
        private readonly IClaimsService _claimsService;

        public GroupController(IGroupFundsService groupFundsService, IClaimsService claimsService)
        {
            _groupFundsService = groupFundsService;
            _claimsService = claimsService;
        }

        [HttpDelete("{groupId}/members/{memberId}")]
        public async Task<IActionResult> RemoveMemberAsync(Guid groupId, Guid memberId)
        {
            return await ValidateAndExecute(() => _groupFundsService.RemoveMemberByLeaderAsync(groupId, memberId));
        }

        [HttpGet("members/leave")]
        public async Task<IActionResult> LeaveGroupAsync([FromQuery] Guid groupId)
        {
            return await ValidateAndExecute(() => _groupFundsService.LeaveGroupAsync(groupId));
        }

        [HttpPut("members/role")]
        public async Task<IActionResult> SetMemberRoleAsync(SetRoleGroupModel setRoleGroupModel)
        {
            return await ValidateAndExecute(() => _groupFundsService.SetMemberRoleAsync(setRoleGroupModel));
        }

        [HttpPost("invite-member/email")]
        [Authorize]
        public async Task<IActionResult> InviteMemberEmailAsync([FromBody] InviteMemberModel inviteMemberModel)
        {
            return await ValidateAndExecute(() => _groupFundsService.InviteMemberEmailAsync(inviteMemberModel));
        }

        [HttpPost("invite-member/qrcode")]
        [Authorize]
        public async Task<IActionResult> InviteMemberQRCodeAsync([FromBody] InviteMemberModel inviteMemberModel)
        {
            return await ValidateAndExecute(() => _groupFundsService.InviteMemberQRCodeAsync(inviteMemberModel));
        }


        [HttpGet("invite-member/email/accept")]
        public async Task<IActionResult> AcceptInvitationEmailAsync([FromQuery] string token)
        {
            return await ValidateAndExecute(() => _groupFundsService.AcceptInvitationEmailAsync(token));
        }

        [HttpGet("invite-member/qrcode/accept")]
        public async Task<IActionResult> AcceptInvitationQRCodeAsync([FromQuery] string token)
        {
            return await ValidateAndExecute(() => _groupFundsService.AcceptInvitationQRCodeAsync(token));
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroupFund([FromBody] CreateGroupModel model)
        {
            return await ValidateAndExecute(() => _groupFundsService.CreateGroupFundsAsync(model));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGroupFunds([FromQuery] PaginationParameter paginationParameters)
        {
            return await ValidateAndExecute(() => _groupFundsService.GetAllGroupFunds(paginationParameters));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupFundById(Guid id)
        {
            return await ValidateAndExecute(() => _groupFundsService.GetGroupFundById(id));
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DisbandGroupFund(Guid groupId)
        {
            return await ValidateAndExecute(() => _groupFundsService.CloseGroupFundAsync(groupId));
        }

        [HttpPut("contribution")]
        public async Task<IActionResult> SetGroupContribution([FromBody] SetGroupContributionModel setGroupContributionModel)
        {
            return await ValidateAndExecute(() => _groupFundsService.SetGroupContribution(setGroupContributionModel));
        }
    }
}
