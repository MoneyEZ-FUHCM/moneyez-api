using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class BudgetModelsSubcategory : BaseEntity
{
    public Guid? BudgetModelId { get; set; }

    public Guid? SubcategoryId { get; set; }

    public virtual BudgetModel? BudgetModel { get; set; }

    public virtual Subcategory? Subcategory { get; set; }
}
