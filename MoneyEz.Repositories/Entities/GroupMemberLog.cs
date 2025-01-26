#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class GroupMemberLog : BaseEntity
{
    public Guid GroupMemberId { get; set; }

    public int? ChangeType { get; set; }

    public DateTime? ChangedAt { get; set; }

    public virtual GroupMember GroupMember { get; set; }
}