﻿using System;
using MoneyEz.Repositories.Enums;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Transaction : BaseEntity
{
    public Guid? GroupId { get; set; }

    public Guid? UserId { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public Guid? SubcategoryId { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? Description { get; set; }

    public bool? ApprovalRequired { get; set; }

    public string? RequestCode { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.PENDING; // default is PENDING

    public Guid? UserSpendingModelId { get; set; }

    public InsertType InsertType { get; set; } = InsertType.MANUAL; // default is MANUAL

    public virtual GroupFund? Group { get; set; }

    public virtual Subcategory? Subcategory { get; set; }

    public virtual ICollection<TransactionVote> TransactionVotes { get; set; } = new List<TransactionVote>();

    public virtual User? User { get; set; }
}