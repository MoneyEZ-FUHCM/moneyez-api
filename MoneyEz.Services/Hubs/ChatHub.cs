using Microsoft.AspNetCore.SignalR;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.ChatHistoryModels;
using MoneyEz.Services.Services.Implements;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatHistoryService _chatHistoryService;
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(IChatHistoryService chatHistoryService, IUnitOfWork unitOfWork) 
        { 
            _chatHistoryService = chatHistoryService;
            _unitOfWork = unitOfWork;
        }
        public async Task SendMessage(string user, string message)
        {
            // save message user
            var userMessage = new CreateChatHistoryModel
            {
                Email = user,
                Message = message,
                MessageType = MessageType.USER
            };
            await _chatHistoryService.CreateAndUpdateConversation(userMessage);

            string botResponse = ProcessMessage(message);

            var botMessage = new CreateChatHistoryModel
            {
                Email = user,
                Message = botResponse,
                MessageType = MessageType.BOT
            };

            await _chatHistoryService.CreateAndUpdateConversation(botMessage);

            await Clients.Caller.SendAsync("ReceiveMessage", "MoneyEzAssistant", botResponse, CommonUtils.GetCurrentTime());
        }

        private string ProcessMessage(string message)
        {
            message = message.ToLower();

            if (message.Contains("hello") || message.Contains("hi"))
            {
                return "Xin chào! Tôi là chatbot, tôi có thể giúp gì cho bạn?";
            }
            else if (message.Contains("hôm nay ngày mấy"))
            {
                return $"Hôm nay là {DateTime.Now:dddd, dd/MM/yyyy}.";
            }
            else if (message.Contains("tạm biệt"))
            {
                return "Tạm biệt! Hẹn gặp lại bạn sau.";
            }
            else
            {
                return "Xin lỗi, tôi chưa hiểu yêu cầu của bạn. Bạn có thể hỏi lại theo cách khác không?";
            }
        }
    }
}
