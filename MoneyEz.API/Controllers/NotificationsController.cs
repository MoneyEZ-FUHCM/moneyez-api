using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.NotificationModels;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;
using Newtonsoft.Json;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/notifications")]
    [ApiController]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly IClaimsService _claimsService;

        public NotificationsController(INotificationService notificationService, IClaimsService claimsService) 
        {
            _notificationService = notificationService;
            _claimsService = claimsService;
        }

        [HttpGet("user")]
        [Authorize]
        public Task<IActionResult> GetNotificationsByUser([FromQuery] PaginationParameter paginationParameter, [FromQuery] NotificationFilter filter)
        {
            return ValidateAndExecute(() => _notificationService.GetNotificationsByUser(paginationParameter, filter));
        }

        [HttpGet("{id}")]
        [Authorize]
        public Task<IActionResult> GetNotificationById(Guid id)
        {
            return ValidateAndExecute(() => _notificationService.GetNotificationById(id));
        }

        [HttpGet("mark-all-isread")]
        [Authorize]
        public Task<IActionResult> MarkAllUserNotificationIsRead()
        {
            string currentEmail = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _notificationService.MarkAllUserNotificationIsReadAsync(currentEmail));
        }

        [HttpGet("{notificationId}/mark-isread")]
        [Authorize]
        public Task<IActionResult> MarkAllUserNotificationIsRead(Guid notificationId)
        {
            return ValidateAndExecute(() => _notificationService.MarkNotificationIsReadById(notificationId));
        }

        [HttpPost("add-notification-for-users")]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> AddNotificationForUsers(CreateNotificationModel createNotificationModel)
        {
            return ValidateAndExecute(() => _notificationService.AddNotificationByAdmin(createNotificationModel));
        }
    }
}
