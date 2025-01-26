#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class UserSetting : BaseEntity
{
    public Guid? UserId { get; set; }

    public string UserSettingKey { get; set; }

    public string UserSettingValue { get; set; }

    public virtual User User { get; set; }
}