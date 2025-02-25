using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.API.ViewModels.RequestModels;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/chats")]
    [ApiController]
    public class ChatsController : BaseController
    {
        private readonly IChatHistoryService _chatHistoryService;
        private readonly IClaimsService _claimsService;

        public ChatsController(IChatHistoryService chatHistoryService, IClaimsService claimsService) 
        {
            _chatHistoryService = chatHistoryService;
            _claimsService = claimsService;
        }

        [HttpGet("conversation")]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> GetChatHistoriesPaging([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _chatHistoryService.GetChatHistoriesPaging(paginationParameter));
        }

        [HttpGet("conversation/messages/user")]
        [Authorize]
        public Task<IActionResult> GetChatMessagePaging([FromQuery] PaginationParameter paginationParameter)
        {
            string currentEmail = _claimsService.GetCurrentUserEmail;
            return ValidateAndExecute(() => _chatHistoryService.GetChatMessageConversation(paginationParameter, currentEmail));
        }
    }
}
