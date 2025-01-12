using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Transaction : BaseEntity
{
    public int? Type { get; set; }

    public Guid? GroupId { get; set; }

    public int? Amount { get; set; }

    public string? Description { get; set; }

    public Guid? UserId { get; set; }

    public Guid? SubcategoryId { get; set; }

    public virtual Group? Group { get; set; }

    public virtual Subcategory? Subcategory { get; set; }

    public virtual User? User { get; set; }
}
