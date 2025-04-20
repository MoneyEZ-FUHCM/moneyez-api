using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ExternalServiceModels;
using MoneyEz.Services.BusinessModels.QuizModels;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IExternalApiService
    {
        Task<ChatMessageExternalResponse> ProcessMessageAsync(ChatMessageRequest request);

        Task<BaseResultModel> ExecuteReceiveExternalService(ExternalReciveRequestModel model);

        Task<BaseResultModel> ExecuteSendExternalService(ExternalSendRequestModel model);

        Task<RecomendModelResponse> SuggestionSpendingModelSerivce(List<QuestionAnswerPair> answerPairs);

        Task<BaseResultModel> SuggestionSpendingModelSerivceTest(List<QuestionAnswerPair> answerPairs);

        Task<BaseResultModel> GetKnowledgeDocuments(PaginationParameter paginationParameter);

        Task<BaseResultModel> ExecuteCreateKnownledgeDocument(IFormFile file);

        Task<BaseResultModel> ExecuteDeleteKnownledgeDocument(Guid id);
    }
}
