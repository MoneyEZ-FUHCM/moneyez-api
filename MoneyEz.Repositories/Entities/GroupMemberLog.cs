using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class GroupMemberLog : BaseEntity
{
    public Guid GroupMemberId { get; set; }

    public GroupAction? ChangeType { get; set; }

    public string? ChangeDiscription { get; set; }

    public virtual GroupMember? GroupMember { get; set; }
}