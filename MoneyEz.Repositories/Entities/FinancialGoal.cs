#nullable disable
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class FinancialGoal : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid? GroupId { get; set; }

    public Guid? SubcategoryId { get; set; }

    public string Name { get; set; }

    public string NameUnsign { get; set; }

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public DateTime Deadline { get; set; }

    public FinancialGoalStatus Status { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; }

    public virtual GroupFund Group { get; set; }

    public virtual User User { get; set; }

    public virtual Subcategory Subcategory { get; set; }
}