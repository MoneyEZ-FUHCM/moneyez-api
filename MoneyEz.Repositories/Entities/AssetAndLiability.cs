#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class AssetAndLiability : BaseEntity
{
    public Guid UserId { get; set; }

    public string Name { get; set; }

    public string NameUnsign { get; set; }

    public int? Type { get; set; }

    public Guid SubcategoryId { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string Description { get; set; }

    public int? OwnershipType { get; set; }

    public virtual User User { get; set; }
}