#nullable disable
using System;
using System.Collections.Generic;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.Repositories.Entities;

public partial class GroupMember : BaseEntity
{
    public Guid GroupId { get; set; }

    public Guid UserId { get; set; }

    public decimal? ContributionPercentage { get; set; }

    public RoleGroup? Role { get; set; }

    public GroupMemberStatus? Status { get; set; }

    public virtual GroupFund Group { get; set; }

    public virtual User User { get; set; }
}