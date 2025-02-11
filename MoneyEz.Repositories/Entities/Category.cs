using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Category : BaseEntity
{
    public string? Name { get; set; }

    public string? NameUnsign { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<SpendingModelCategory> SpendingModelCategories { get; set; } = new List<SpendingModelCategory>();

    public virtual ICollection<CategorySubcategory> CategorySubcategories { get; set; } = new List<CategorySubcategory>();
}