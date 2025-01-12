using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class CategoryBudgetModel : BaseEntity
{
    public Guid? BudgetModelId { get; set; }

    public Guid? CategoryId { get; set; }

    public int? BudgetAmount { get; set; }

    public virtual BudgetModel? BudgetModel { get; set; }

    public virtual Category? Category { get; set; }
}
