﻿#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class FinancialReport : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid? GroupId { get; set; }

    public string Name { get; set; }

    public string NameUnsign { get; set; }

    public int? ReportType { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal NetBalance { get; set; }

    public virtual User User { get; set; }

    public virtual GroupFund GroupFund { get; set; }
}