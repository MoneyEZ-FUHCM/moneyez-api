#nullable disable
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class RecurringTransaction : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid SubcategoryId { get; set; }

    public TransactionType Type { get; set; }

    public string Tags { get; set; }

    public decimal Amount { get; set; }

    public FrequencyType FrequencyType { get; set; }

    public int Interval { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Description { get; set; }

    public CommonsStatus Status { get; set; }

    public virtual Subcategory Subcategory { get; set; }

    public virtual User User { get; set; }
}