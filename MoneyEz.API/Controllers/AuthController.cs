using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.API.ViewModels.RequestModels;
using MoneyEz.Services.BusinessModels.AuthenModels;
using MoneyEz.Services.BusinessModels.OtpModels;
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
        private readonly IClaimsService _claimsService;

        public AuthController(IUserService userService, IClaimsService claimsService)
        {
            _userService = userService;
            _claimsService = claimsService;
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

        [HttpPost("refresh-token")]
        public Task<IActionResult> RefreshToken([FromBody] string token)
        {
            return ValidateAndExecute(() => _userService.RefreshToken(token));
        }

        [HttpPost("verify-email")]
        public Task<IActionResult> VerifyEmail([FromBody] ConfirmOtpModel confirmOtpModel)
        {
            return ValidateAndExecute(() => _userService.VerifyEmail(confirmOtpModel));
        }

        [HttpPost("reset-password/request")]
        public Task<IActionResult> RequestResetPassword([FromBody] string email)
        {
            return ValidateAndExecute(() => _userService.RequestResetPassword(email));
        }

        [HttpPost("reset-password/confirm")]
        public Task<IActionResult> ConfirmResetPassword(ConfirmOtpModel confirmOtpModel)
        {
            return ValidateAndExecute(() => _userService.ConfirmResetPassword(confirmOtpModel));
        }

        [HttpPost("reset-password/new-password")]
        public Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            return ValidateAndExecute(() => _userService.ExecuteResetPassword(resetPasswordModel));
        }

        [HttpPost("change-password")]
        public Task<IActionResult> ChangePassword(ChangePasswordModel changePasswordModel)
        {
            var email = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _userService.ChangePasswordAsync(email, changePasswordModel));
        }
    }
}
