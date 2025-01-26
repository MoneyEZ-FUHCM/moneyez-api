#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class ChatMessage : BaseEntity
{
    public Guid? ChatHistoryId { get; set; }

    public int? Type { get; set; }

    public string Message { get; set; }

    public string MessageUnsign { get; set; }

    public virtual ChatHistory ChatHistory { get; set; }
}