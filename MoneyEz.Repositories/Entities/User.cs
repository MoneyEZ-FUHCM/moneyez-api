using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class User : BaseEntity
{
    public string? FullName { get; set; }

    public string? NameUnsign { get; set; }

    public required string Email { get; set; }

    public string? Password { get; set; }

    public DateTime? Dob { get; set; }

    public Gender? Gender { get; set; }

    public string? Address { get; set; } 

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? GoogleId { get; set; }

    public bool? IsVerified { get; set; }

    public RolesEnum? Role { get; set; }

    public string? DeviceToken { get; set; }

    public CommonsStatus? Status { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual ICollection<Liability> Liabilities { get; set; } = new List<Liability>();

    public virtual ICollection<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();

    public virtual ICollection<FinancialGoal> FinancialGoals { get; set; } = new List<FinancialGoal>();

    public virtual ICollection<FinancialReport> FinancialReports { get; set; } = new List<FinancialReport>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserQuizResult> UserQuizResults { get; set; } = new List<UserQuizResult>();

    public virtual ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();

    public virtual ICollection<UserSpendingModel> UserSpendingModels { get; set; } = new List<UserSpendingModel>();
}