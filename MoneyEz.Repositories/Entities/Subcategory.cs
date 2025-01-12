using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Subcategory : BaseEntity
{
    public Guid? CategoryId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? IconName { get; set; }

    public virtual ICollection<BudgetModelsSubcategory> BudgetModelsSubcategories { get; set; } = new List<BudgetModelsSubcategory>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<FixedTransaction> FixedTransactions { get; set; } = new List<FixedTransaction>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
