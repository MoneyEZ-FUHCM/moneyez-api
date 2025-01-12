using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class PlanSetting : BaseEntity
{
    public Guid? PlanId { get; set; }

    public string? SettingKey { get; set; }

    public string? SettingValue { get; set; }
}
