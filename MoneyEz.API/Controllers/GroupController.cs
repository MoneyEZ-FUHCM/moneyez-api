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
using MoneyEz.Repositories.Commons.Filters;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/groups")]
    [ApiController]
    public class GroupController : BaseController
    {
        private readonly IGroupFundsService _groupFundsService;
        private readonly IGroupTransactionService _groupTransactionService;
        private readonly IGroupMemberService _groupMemberService;
        private readonly IClaimsService _claimsService;

        public GroupController(IGroupFundsService groupFundsService,
            IGroupTransactionService groupTransactionService,
            IGroupMemberService groupMemberService,
            IClaimsService claimsService)
        {
            _groupFundsService = groupFundsService;
            _groupTransactionService = groupTransactionService;
            _groupMemberService = groupMemberService;
            _claimsService = claimsService;
        }

        #region group members

        [HttpDelete("{groupId}/members/{memberId}")]
        [Authorize]
        public async Task<IActionResult> RemoveMemberAsync(Guid groupId, Guid memberId)
        {
            return await ValidateAndExecute(() => _groupMemberService.RemoveMemberByLeaderAsync(groupId, memberId));
        }

        [HttpGet("members/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveGroupAsync([FromQuery] Guid groupId)
        {
            return await ValidateAndExecute(() => _groupMemberService.LeaveGroupAsync(groupId));
        }

        [HttpPut("members/role")]
        [Authorize]
        public async Task<IActionResult> SetMemberRoleAsync(SetRoleGroupModel setRoleGroupModel)
        {
            return await ValidateAndExecute(() => _groupMemberService.SetMemberRoleAsync(setRoleGroupModel));
        }

        [HttpPost("invite-member/email")]
        [Authorize]
        public async Task<IActionResult> InviteMemberEmailAsync([FromBody] InviteMemberModel inviteMemberModel)
        {
            return await ValidateAndExecute(() => _groupMemberService.InviteMemberEmailAsync(inviteMemberModel));
        }

        [HttpPost("invite-member/qrcode")]
        [Authorize]
        public async Task<IActionResult> InviteMemberQRCodeAsync([FromBody] InviteMemberModel inviteMemberModel)
        {
            return await ValidateAndExecute(() => _groupMemberService.InviteMemberQRCodeAsync(inviteMemberModel));
        }


        [HttpGet("invite-member/email/accept")]
        public async Task<IActionResult> AcceptInvitationEmailAsync([FromQuery] string token)
        {
            return await ValidateAndExecute(() => _groupMemberService.AcceptInvitationEmailAsync(token));
        }

        [HttpGet("invite-member/qrcode/accept")]
        [Authorize]
        public async Task<IActionResult> AcceptInvitationQRCodeAsync([FromQuery] string token)
        {
            return await ValidateAndExecute(() => _groupMemberService.AcceptInvitationQRCodeAsync(token));
        }

        [HttpPut("contribution")]
        [Authorize]
        public async Task<IActionResult> SetGroupContribution([FromBody] SetGroupContributionModel setGroupContributionModel)
        {
            return await ValidateAndExecute(() => _groupMemberService.SetGroupContribution(setGroupContributionModel));
        }

        #endregion

        #region group management

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateGroupFund([FromBody] CreateGroupModel model)
        {
            return await ValidateAndExecute(() => _groupFundsService.CreateGroupFundsAsync(model));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllGroupFunds([FromQuery] PaginationParameter paginationParameters, [FromQuery] GroupFilter groupFilter)
        {
            return await ValidateAndExecute(() => _groupFundsService.GetAllGroupFunds(paginationParameters, groupFilter));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetGroupFundById(Guid id)
        {
            return await ValidateAndExecute(() => _groupFundsService.GetGroupFundById(id));
        }

        [Authorize]
        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DisbandGroupFund(Guid groupId)
        {
            return await ValidateAndExecute(() => _groupFundsService.CloseGroupFundAsync(groupId));
        }

        [HttpGet("logs/{id}")]
        [Authorize]
        public async Task<IActionResult> GetGroupFundLogs(Guid id, [FromQuery]PaginationParameter paginationParameters, [FromQuery]GroupLogFilter filter)
        {
            return await ValidateAndExecute(() => _groupFundsService.GetGroupFundLogs(id, paginationParameters, filter));
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateGroupFund([FromBody] UpdateGroupModel model)
        {
            return await ValidateAndExecute(() => _groupFundsService.UpdateGroupFundsAsync(model));
        }

        #endregion

        #region group transactions

        [HttpPost("fund-raising/request")]
        [Authorize]
        public async Task<IActionResult> CreateFundraisingRequest([FromBody] CreateFundraisingModel model)
        {
            return await ValidateAndExecute(() => _groupTransactionService.CreateFundraisingRequest(model));
        }

        [HttpPost("fund-withdraw/request")]
        [Authorize]
        public async Task<IActionResult> CreateFundWithdrawalRequest([FromBody] CreateFundWithdrawalModel model)
        {
            return await ValidateAndExecute(() => _groupTransactionService.CreateFundWithdrawalRequest(model));
        }

        [HttpPost("fund-raising/remind")]
        [Authorize]
        public async Task<IActionResult> CreateFundraisingRemind([FromBody] RemindFundraisingModel model)
        {
            return await ValidateAndExecute(() => _groupTransactionService.RemindFundraisingAsync(model));
        }

        [HttpGet("pending-requests")]
        [Authorize]
        public async Task<IActionResult> GetPendingRequests([FromQuery] Guid groupId, [FromQuery] PaginationParameter paginationParameters)
        {
            return await ValidateAndExecute(() => _groupTransactionService.GetPendingRequestsAsync(groupId, paginationParameters));
        }

        [HttpGet("pending-requests/{requestId}")]
        [Authorize]
        public async Task<IActionResult> GetPendingRequestDetail([FromRoute] Guid requestId)
        {
            return await ValidateAndExecute(() => _groupTransactionService.GetPendingRequestDetailAsync(requestId));
        }

        #endregion
    }
}
