using System.Text.Json.Serialization;

namespace MoneyEz.Services.BusinessModels.ChatModels
{
    public class ChatMessageResponseModel
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("conversation_id")]
        public string ConversationId { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public MessageContent Message { get; set; } = new();
    }

    public class MessageContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public List<ContentItem> Content { get; set; } = new();
    }

    public class ContentItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}