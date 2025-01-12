using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Payment : BaseEntity
{
    public Guid? SubscriptionId { get; set; }

    public string? PaymentCode { get; set; }

    public string? TransactionCode { get; set; }

    public int? Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public int? PaymentMethod { get; set; }

    public int? Status { get; set; }

    public virtual Subscription? Subscription { get; set; }
}
