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
            var result = await _groupFundsService.RemoveMemberAsync(groupId, memberId);
            if (result.Status == StatusCodes.Status200OK)
            {
                return Ok(result);
            }
            return StatusCode(result.Status, result);
        }

        [HttpPut("{groupId}/members/{memberId}/role")]
        public async Task<IActionResult> SetMemberRoleAsync(Guid groupId, Guid memberId, [FromBody] RoleGroup newRole)
        {
            var result = await _groupFundsService.SetMemberRoleAsync(groupId, memberId, newRole);
            if (result.Status == StatusCodes.Status200OK)
            {
                return Ok(result);
            }
            return StatusCode(result.Status, result);
        }

        [HttpPost("invite-member/email")]
        [Authorize]
        public async Task<IActionResult> InviteMemberEmailAsync([FromBody] InviteMemberModel inviteMemberModel)
        {
            var result = await _groupFundsService.InviteMemberEmailAsync(inviteMemberModel);
            if (result.Status == StatusCodes.Status200OK)
            {
                return Ok(result);
            }
            return StatusCode(result.Status, result);
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
    }
}
