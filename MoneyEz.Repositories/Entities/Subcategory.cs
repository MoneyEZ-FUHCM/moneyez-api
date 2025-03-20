using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Subcategory : BaseEntity
{
    public string? Name { get; set; }

    public string? NameUnsign { get; set; }

    public string? Description { get; set; }

    public string? Code { get; set; }

    public string? Icon { get; set; }

    public virtual ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<CategorySubcategory> CategorySubcategories { get; set; } = new List<CategorySubcategory>();
}