#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class SpendingModelCategory : BaseEntity
{
    public Guid? SpendingModelId { get; set; }

    public Guid? CategoryId { get; set; }

    public decimal? PercentageAmount { get; set; }

    public virtual Category Category { get; set; }

    public virtual SpendingModel SpendingModel { get; set; }
}