#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Subscription : BaseEntity
{
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Price { get; set; }

    public int? Status { get; set; }

    public Guid? UserId { get; set; }

    public Guid? SubscriptionPlanId { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual SubscriptionPlan SubscriptionPlan { get; set; }

    public virtual User User { get; set; }
}