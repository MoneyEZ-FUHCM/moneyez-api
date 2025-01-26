using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.BusinessModels.GroupMember;
using MoneyEz.Services.Services.Interfaces;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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


    }
}