using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Category : BaseEntity
{
    public string? Name { get; set; }

    public string? IconName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<CategoryBudgetModel> CategoryBudgetModels { get; set; } = new List<CategoryBudgetModel>();

    public virtual ICollection<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
}
