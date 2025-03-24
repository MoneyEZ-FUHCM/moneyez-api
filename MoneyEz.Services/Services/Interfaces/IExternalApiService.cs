using MoneyEz.Services.BusinessModels.ChatModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IExternalApiService
    {
        Task<ChatMessageExternalResponse> ProcessMessageAsync(ChatMessageRequest request);
    }
}
