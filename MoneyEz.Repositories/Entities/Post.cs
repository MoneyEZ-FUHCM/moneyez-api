using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Post : BaseEntity
{
    public string? Title { get; set; }

    public string? Content { get; set; }
}
