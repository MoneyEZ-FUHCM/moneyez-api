using MoneyEz.Services.BusinessModels.ChatHistoryModels;

namespace MoneyEz.Services.BusinessModels.ChatModels
{
    public class ChatMessageRequest
    {
        public string Message { get; set; } = "";
        public Guid UserId { get; set; }
        public Guid ConversationId { get; set; }
        public List<SendChatToExternalModel> PreviousMessages = new List<SendChatToExternalModel>();
    }

    public class ChatMessageResponse
    {
        public string? Message { get; set; }
    }

    public class ChatMessageExternalResponse
    {
        public bool? IsSuccess { get; set; }
        public string? Message { get; set; }
    }

    public class ChatResponseCreateTransaction
    {
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
    }
}
