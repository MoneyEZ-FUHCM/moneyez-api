﻿#nullable disable
using System;
using System.Collections.Generic;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.Repositories.Entities;

public partial class GroupFund : BaseEntity
{
    public string Name { get; set; }

    public string NameUnsign { get; set; }

    public string Description { get; set; }

    public decimal CurrentBalance { get; set; }

    public GroupStatus? Status { get; set; }

    public VisibilityEnum? Visibility { get; set; }

    public Guid? AccountBankId { get; set; }

    public virtual ICollection<FinancialGoal> FinancialGoals { get; set; } = new List<FinancialGoal>();

    public virtual ICollection<FinancialReport> FinancialReports { get; set; } = new List<FinancialReport>();

    public virtual ICollection<GroupFundLog> GroupFundLogs { get; set; } = new List<GroupFundLog>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}