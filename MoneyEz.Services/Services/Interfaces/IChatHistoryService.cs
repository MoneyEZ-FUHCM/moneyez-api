using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.ChatHistoryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IChatHistoryService
    {
        public Task<ChatHistoryModel> CreateAndUpdateConversation(CreateChatHistoryModel model);

        public Task<ChatHistoryModel> AddMessageToConversation(CreateChatHistoryModel model);
    }
}
