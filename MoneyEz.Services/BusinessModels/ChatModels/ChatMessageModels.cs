namespace MoneyEz.Services.BusinessModels.ChatModels
{
    public class ChatMessageRequest
    {
        public string Message { get; set; } = "";
        public string Language { get; set; } = "vi"; // Default to Vietnamese
    }

    public class ChatMessageResponse
    {
        public string? Message { get; set; }
    }

    public class ChatMessageExternalResponse
    {
        public int? HttpCode { get; set; }
        public string? Message { get; set; }
        public string? Command { get; set; }
        public ChatResponseCreateTransaction? Data { get; set; }
    }

    public class ChatResponseCreateTransaction
    {
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
    }
}
