#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class PlanSetting : BaseEntity
{
    public string PlanSettingKey { get; set; }

    public string PlanSettingValue { get; set; }

    public Guid? SubscriptionPlanId { get; set; }

    public virtual SubscriptionPlan SubscriptionPlan { get; set; }
}