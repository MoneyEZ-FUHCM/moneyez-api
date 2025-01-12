using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Group : BaseEntity
{
    public string? Avatar { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? SubscriptionStatus { get; set; }

    public Guid? SubscriptionPlanId { get; set; }

    public int? Status { get; set; }

    public bool? IsPersonal { get; set; }

    public virtual ICollection<BudgetModel> BudgetModels { get; set; } = new List<BudgetModel>();

    public virtual ICollection<Target> Targets { get; set; } = new List<Target>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
