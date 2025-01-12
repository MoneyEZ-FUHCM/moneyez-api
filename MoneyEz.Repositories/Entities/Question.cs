using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Question : BaseEntity
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}
