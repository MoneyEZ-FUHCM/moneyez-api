using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace MoneyEz.Repositories.Entities;

public partial class Category : BaseEntity
{
    public string? Name { get; set; }

    public string? NameUnsign { get; set; }

    public string? Description { get; set; }

    public string? Code { get; set; }

    public string? Icon { get; set; }

    public TransactionType? Type { get; set; }

    public virtual ICollection<SpendingModelCategory> SpendingModelCategories { get; set; } = new List<SpendingModelCategory>();

    public virtual ICollection<CategorySubcategory> CategorySubcategories { get; set; } = new List<CategorySubcategory>();
}