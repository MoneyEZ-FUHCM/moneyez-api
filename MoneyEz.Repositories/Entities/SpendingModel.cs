#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class SpendingModel : BaseEntity
{
    public string Name { get; set; }

    public string NameUnsign { get; set; }

    public string Description { get; set; }

    public bool? IsTemplate { get; set; }

    public virtual ICollection<SpendingModelCategory> SpendingModelCategories { get; set; } = new List<SpendingModelCategory>();

    public virtual ICollection<UserSpendingModel> UserSpendingModels { get; set; } = new List<UserSpendingModel>();
}