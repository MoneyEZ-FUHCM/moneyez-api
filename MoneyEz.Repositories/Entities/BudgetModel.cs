using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class BudgetModel : BaseEntity
{
    public Guid? GroupId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? IsTemplate { get; set; }

    public int? Status { get; set; }

    public int? PeriodUnit { get; set; }

    public int? PeriodValue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual ICollection<BudgetModelsSubcategory> BudgetModelsSubcategories { get; set; } = new List<BudgetModelsSubcategory>();

    public virtual ICollection<CategoryBudgetModel> CategoryBudgetModels { get; set; } = new List<CategoryBudgetModel>();

    public virtual Group? Group { get; set; }
}
