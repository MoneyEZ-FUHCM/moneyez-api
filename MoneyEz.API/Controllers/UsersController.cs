﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.UserModels;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IClaimsService _claimsService;

        public UsersController(IUserService userService, IClaimsService claimsService)
        {
            _userService = userService;
            _claimsService = claimsService;
        }

        [HttpGet]
        [Authorize]
        public Task<IActionResult> GetUsers([FromQuery] PaginationParameter paginationParameter, [FromQuery] UserFilter userFilter)
        {
            return ValidateAndExecute(() => _userService.GetUserPaginationAsync(paginationParameter, userFilter));
        }

        [HttpGet("{id}")]
        [Authorize]
        public Task<IActionResult> GetUserById(Guid id)
        {
            return ValidateAndExecute(() => _userService.GetUserByIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> CreateUser(CreateUserModel model)
        {
            return ValidateAndExecute(() => _userService.CreateUserAsync(model));
        }

        [HttpPut]
        [Authorize]
        public Task<IActionResult> UpdateUser(UpdateUserModel model)
        {
            return ValidateAndExecute(() => _userService.UpdateUserAsync(model));
        }

        [HttpPut("ban/{id}")]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> BanUser(Guid id)
        {
            string currentEmail = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _userService.BanUserAsync(id, currentEmail));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> DeleteUser(Guid id)
        {
            string currentEmail = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _userService.DeleteUserAsync(id, currentEmail));
        }

        [HttpPut("update-fcm-token")]
        [Authorize]
        public Task<IActionResult> UpdateDeviceToken([FromBody] string fcmToken)
        {
            string currentEmail = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _userService.UpdateFcmTokenAsync(currentEmail, fcmToken));
        }

        [HttpGet("current")]
        [Authorize]
        public Task<IActionResult> GetCurrentUser()
        {
            string currentEmail = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _userService.GetCurrentUser(currentEmail));
        }

    }
}
