#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class GroupFundLog : BaseEntity
{
    public Guid GroupId { get; set; }

    public string ChangeDescription { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual GroupFund Group { get; set; }
}