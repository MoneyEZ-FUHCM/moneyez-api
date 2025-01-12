using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class ChatHistory : BaseEntity
{
    public Guid? UserId { get; set; }

    public string? Name { get; set; }

    public string? Content { get; set; }
}
