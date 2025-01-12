using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Notification : BaseEntity
{
    public int? Type { get; set; }

    public bool? IsRead { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public Guid? EntityId { get; set; }

    public string? EntityName { get; set; }

    public Guid? UserId { get; set; }

    public virtual User? User { get; set; }
}
