using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ExternalServiceModels;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IExternalApiService
    {
        Task<ChatMessageExternalResponse> ProcessMessageAsync(ChatMessageRequest request);

        Task<BaseResultModel> ExecuteReceiveExternalService(ExternalReciveRequestModel model);

        Task<BaseResultModel> ExecuteSendExternalService(ExternalSendRequestModel model);

        Task<BaseResultModel> ExecuteKnownledgeDocumentSerivce(ExternalKnowledgeRequestModel model, PaginationParameter paginationParameter);
    }
}
