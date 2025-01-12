using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class User : BaseEntity
{
    public string? Avatar { get; set; }

    public int? Role { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime? Dob { get; set; }

    public string? Address { get; set; }

    public string? PasswordHash { get; set; }

    public int? Gender { get; set; }

    public virtual ICollection<FixedTransaction> FixedTransactions { get; set; } = new List<FixedTransaction>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();
}
