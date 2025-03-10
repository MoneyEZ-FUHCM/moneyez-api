﻿using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
public class PersonalFinancialGoalModel : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SubcategoryId { get; set; }
    public string Name { get; set; }
    public string NameUnsign { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; } = 0;
    public FinancialGoalStatus Status { get; set; }
    public DateTime Deadline { get; set; }
}
}