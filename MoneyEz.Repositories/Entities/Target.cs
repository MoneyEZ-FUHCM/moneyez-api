using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Target : BaseEntity
{
    public string? Name { get; set; }

    public int? Goal { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public Guid? GroupId { get; set; } // Nullable for user-specific targets

    public virtual Group? Group { get; set; }
}
