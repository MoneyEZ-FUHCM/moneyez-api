#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class TransactionVote : BaseEntity
{
    public Guid TransactionId { get; set; }

    public Guid UserId { get; set; }

    public bool? Vote { get; set; }

    public virtual Transaction Transaction { get; set; }
}