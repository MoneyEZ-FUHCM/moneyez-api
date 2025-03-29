using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.ChatHistoryModels
{
    public class ChatMessageModel : BaseEntity
    {
        public Guid? ChatHistoryId { get; set; }

        public int? Type { get; set; }

        public string Message { get; set; } = "";

    }

    public class SendChatToExternalModel
    {
        public Guid ConversationId { get; set; }
        public string Content { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
