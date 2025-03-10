﻿#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class ChatHistory : BaseEntity
{
    public string Intent { get; set; }

    public string IntentUnsign { get; set; }

    public Guid? UserId { get; set; }

    public Guid? RoomNo { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual User User { get; set; }
}