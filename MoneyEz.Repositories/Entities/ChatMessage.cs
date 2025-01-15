using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Entities
{
    public partial class ChatMessage : BaseEntity
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public Guid? ChatHistoryId { get; set; }

        public virtual ChatHistory ChatHistory { get; set; }
    }
}
