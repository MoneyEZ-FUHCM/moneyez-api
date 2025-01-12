using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class UserSetting : BaseEntity
{
    public Guid? UserId { get; set; }

    public string? SettingKey { get; set; }

    public string? SettingValue { get; set; }

    public virtual User? User { get; set; }
}
