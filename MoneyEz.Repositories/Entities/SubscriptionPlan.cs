﻿#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; }

    public int? Price { get; set; }

    public int? Status { get; set; }

    public string Description { get; set; }

    public virtual ICollection<PlanSetting> PlanSettings { get; set; } = new List<PlanSetting>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}