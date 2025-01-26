#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class UserSpendingModel : BaseEntity
{
    public Guid? UserId { get; set; }

    public Guid? SpendingModelId { get; set; }

    public int? PeriodUnit { get; set; }

    public int? PeriodValue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual SpendingModel SpendingModel { get; set; }

    public virtual User User { get; set; }
}