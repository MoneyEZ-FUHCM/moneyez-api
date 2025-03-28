﻿using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Notification : BaseEntity
{
    public bool IsRead { get; set; } = false;

    public string? Title { get; set; }

    public string? TitleUnsign { get; set; }

    public string? Message { get; set; }

    public string? Href { get; set; }

    public Guid? EntityId { get; set; }

    public Guid? UserId { get; set; }

    public NotificationType? Type { get; set; }

    public virtual User? User { get; set; }
}