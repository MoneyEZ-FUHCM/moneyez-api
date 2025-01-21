using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public Task<IActionResult> GetUsers([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _userService.GetUserPaginationAsync(paginationParameter));
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetUserById(Guid id)
        {
            return ValidateAndExecute(() => _userService.GetUserByIdAsync(id));
        }

    }
}
