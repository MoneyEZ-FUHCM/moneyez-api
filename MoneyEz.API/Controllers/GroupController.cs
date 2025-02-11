using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.GroupMember;
using System;
using System.Threading.Tasks;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupFundsService _groupFundsService;

        public GroupController(IGroupFundsService groupFundsService)
        {
            _groupFundsService = groupFundsService;
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

        [HttpPost("{groupId}/invite")]
        public async Task<IActionResult> InviteMemberAsync(Guid groupId, [FromBody] string email)
        {
            var result = await _groupFundsService.InviteMemberAsync(groupId, email);
            if (result.Status == StatusCodes.Status200OK)
            {
                return Ok(result);
            }
            return StatusCode(result.Status, result);
        }

        [HttpGet("{groupId}/accept-invitation")]
        public async Task<IActionResult> AcceptInvitationAsync(Guid groupId, [FromQuery] string token)
        {
            var result = await _groupFundsService.AcceptInvitationAsync(groupId, token);
            if (result.Status == StatusCodes.Status200OK)
            {
                return Ok(result);
            }
            return StatusCode(result.Status, result);
        }
    }
}
