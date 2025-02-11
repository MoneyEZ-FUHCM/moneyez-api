using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.GroupMember;
using MoneyEz.Services.Services.Interfaces;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [ApiController]
    [Route("api/v1/group-funds")]
    public class GroupFundController : BaseController
    {
        private readonly IGroupFundsService _groupFundService;

        public GroupFundController(IGroupFundsService groupFundService)
        {
            _groupFundService = groupFundService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroupFund([FromBody] CreateGroupModel model)
        {
            return await ValidateAndExecute(() => _groupFundService.CreateGroupFundsAsync(model));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGroupFunds([FromQuery] PaginationParameter paginationParameters)
        {
            return await ValidateAndExecute(() => _groupFundService.GetAllGroupFunds(paginationParameters));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupFundById(Guid id)
        {
            return await ValidateAndExecute(() => _groupFundService.GetGroupFundById(id));
        }

        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DisbandGroupFund(Guid groupId)
        {
            return await ValidateAndExecute(() => _groupFundService.DisbandGroupAsync(groupId));
        }
    }
}