using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.API.ViewModels.RequestModels;
using MoneyEz.Services.BusinessModels.AuthenModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService) 
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public Task<IActionResult> LoginWithEmailPassword(LoginModel loginModel)
        {
            return ValidateAndExecute(() => _userService.LoginWithEmailPassword(loginModel.Email, loginModel.Password));
        }

        [HttpPost("register")]
        public Task<IActionResult> Register(SignUpModel signUpModel)
        {
            return ValidateAndExecute(() => _userService.RegisterAsync(signUpModel));
        }
    }
}
