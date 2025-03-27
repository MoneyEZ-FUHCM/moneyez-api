using System;
using System.Collections.Generic;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.Repositories.Entities;

public partial class GroupFundLog : BaseEntity
{
    public Guid GroupId { get; set; }

    public string? Action { get; set; }

    public string? ChangedBy { get; set; }

    public string? ChangeDescription { get; set; }

    public virtual GroupFund? Group { get; set; }
}