using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class ChatMessage : BaseEntity
{
    public Guid? ChatHistoryId { get; set; }

    public MessageType? Type { get; set; }

    public string? Message { get; set; }

    public virtual ChatHistory? ChatHistory { get; set; }
}