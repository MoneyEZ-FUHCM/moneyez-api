using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.ChatHistoryModels
{
    public class ChatHistoryModel : BaseEntity
    {
        public string Intent { get; set; } = "";

        public string IntentUnsign { get; set; } = "";

        public Guid? UserId { get; set; }

        public Guid? RoomNo { get; set; }

        public virtual ICollection<ChatMessageModel> ChatMessages { get; set; } = new List<ChatMessageModel>();
    }
}
