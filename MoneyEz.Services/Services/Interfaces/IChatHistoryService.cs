using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.ChatHistoryModels;
using MoneyEz.Services.BusinessModels.ResultModels;
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

        public Task<BaseResultModel> GetChatHistoriesPaging(PaginationParameter paginationParameter);

        public Task<BaseResultModel> GetChatMessageConversation(PaginationParameter paginationParameter, string email);

        public Task<BaseResultModel> GetChatMessageHistoriesExternalByUser(Guid userId);
    }
}
