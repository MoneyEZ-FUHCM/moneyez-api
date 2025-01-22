using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.BusinessModels.GroupMember;
using MoneyEz.Services.Services.Interfaces;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupFundController : ControllerBase
    {
        private readonly IGroupFundsService _groupFundService;

        public GroupFundController(IGroupFundsService groupFundService)
        {
            _groupFundService = groupFundService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroupFund([FromBody] CreateGroupModel model)
        {
            var result = await _groupFundService.CreateGroupFundsAsync(model);
            return StatusCode(result.Status, result);
        }


    }
}