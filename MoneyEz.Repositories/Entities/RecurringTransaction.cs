﻿#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class RecurringTransaction : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid SubcategoryId { get; set; }

    public int? Type { get; set; }

    public string Tags { get; set; }

    public decimal Amount { get; set; }

    public int? FrequencyType { get; set; }

    public int Interval { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Description { get; set; }

    public int? Status { get; set; }

    public virtual Subcategory Subcategory { get; set; }

    public virtual User User { get; set; }
}