using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class FixedTransaction : BaseEntity
{
    public Guid? UserId { get; set; }

    public Guid? SubcategoryId { get; set; }

    public string? Title { get; set; }

    public int? Amount { get; set; }

    public string? Description { get; set; }

    public int? PeriodUnit { get; set; }

    public int? PeriodValue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual Subcategory? Subcategory { get; set; }

    public virtual User? User { get; set; }
}
