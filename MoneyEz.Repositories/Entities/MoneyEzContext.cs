#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MoneyEz.Repositories.Entities;

public partial class MoneyEzContext : DbContext
{
    public MoneyEzContext(DbContextOptions<MoneyEzContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ChatHistory> ChatHistories { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<FinancialGoal> FinancialGoals { get; set; }

    public virtual DbSet<FinancialReport> FinancialReports { get; set; }

    public virtual DbSet<GroupFund> GroupFunds { get; set; }

    public virtual DbSet<GroupFundLog> GroupFundLogs { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<GroupMemberLog> GroupMemberLogs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PlanSetting> PlanSettings { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAnswer> QuizAnswers { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizSetting> QuizSettings { get; set; }

    public virtual DbSet<RecurringTransaction> RecurringTransactions { get; set; }

    public virtual DbSet<SpendingModel> SpendingModels { get; set; }

    public virtual DbSet<SpendingModelCategory> SpendingModelCategories { get; set; }

    public virtual DbSet<Subcategory> Subcategories { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionVote> TransactionVotes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserQuizResult> UserQuizResults { get; set; }

    public virtual DbSet<UserSetting> UserSettings { get; set; }

    public virtual DbSet<UserSpendingModel> UserSpendingModels { get; set; }
    
    public virtual DbSet<Image> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Asset__3214EC07");

            entity.ToTable("Asset");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Amount).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.NameUnsign).HasMaxLength(200);
            entity.HasOne(d => d.User).WithMany(p => p.Assets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Asset__UserId");
        });

        modelBuilder.Entity<Liability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Liability__3214EC07");

            entity.ToTable("Liability");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Amount).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.NameUnsign).HasMaxLength(200);
            entity.HasOne(d => d.User).WithMany(p => p.Liabilities)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Liability__UserId");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC077A9D3824");

            entity.ToTable("Category");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.NameUnsign).HasMaxLength(100);
        });

        modelBuilder.Entity<ChatHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatHist__3214EC0718207CD7");

            entity.ToTable("ChatHistory");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Intent).HasMaxLength(100);
            entity.Property(e => e.IntentUnsign).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.ChatHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ChatHisto__UserI__0B91BA14");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatMess__3214EC07066FC8D5");

            entity.ToTable("ChatMessage");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.ChatHistory).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.ChatHistoryId)
                .HasConstraintName("FK__ChatMessa__ChatH__0A9D95DB");
        });

        modelBuilder.Entity<FinancialGoal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Financia__3214EC07E477B7DE");

            entity.ToTable("FinancialGoal");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CurrentAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.NameUnsign).HasMaxLength(100);
            entity.Property(e => e.TargetAmount).HasColumnType("decimal(15, 2)");

            entity.HasOne(d => d.Group).WithMany(p => p.FinancialGoals)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Financial__Group__7D439ABD");

            entity.HasOne(d => d.User).WithMany(p => p.FinancialGoals)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Financial__UserI__7E37BEF6");
        });

        modelBuilder.Entity<FinancialReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Financia__3214EC07A6D22F10");

            entity.ToTable("FinancialReport");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.NameUnsign).HasMaxLength(200);
            entity.Property(e => e.NetBalance).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.TotalExpense).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.TotalIncome).HasColumnType("decimal(15, 2)");
            entity.HasOne(d => d.User).WithMany(p => p.FinancialReports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FinancialReport__UserId");

            entity.HasOne(d => d.GroupFund).WithMany(p => p.FinancialReports)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FinancialReport__GroupFundId");
        });

        modelBuilder.Entity<GroupFund>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GroupFun__3214EC076CA655A0");

            entity.ToTable("GroupFund");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CurrentBalance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.NameUnsign).HasMaxLength(100);
        });

        modelBuilder.Entity<GroupFundLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GroupFun__3214EC0764B86785");

            entity.ToTable("GroupFundLog");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ChangeDescription).IsRequired();
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupFundLogs)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GroupFund__Group__75A278F5");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GroupMem__3214EC07B6FBBF40");

            entity.ToTable("GroupMember");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ContributionPercentage).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GroupMemb__Group__76969D2E");

            entity.HasOne(d => d.User).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GroupMemb__UserI__778AC167");
        });

        modelBuilder.Entity<GroupMemberLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GroupMem__3214EC0785A93823");

            entity.ToTable("GroupMemberLog");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.GroupMember).WithMany(p => p.GroupMemberLogs)
                .HasForeignKey(d => d.GroupMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GroupMemb__Group__787EE5A0");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07F0870137");

            entity.ToTable("Notification");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.TitleUnsign).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__03F0984C");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__3214EC07E8CB354B");

            entity.ToTable("Payment");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PaymentCode).HasMaxLength(255);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.TransactionCode).HasMaxLength(255);

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK__Payment__Subscri__08B54D69");
        });

        modelBuilder.Entity<PlanSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlanSett__3214EC073F36C751");

            entity.ToTable("PlanSetting");

            entity.HasIndex(e => e.PlanSettingKey, "UQ__PlanSett__2C9042B1C79BCF76").IsUnique();

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PlanSettingKey).HasMaxLength(255);

            entity.HasOne(d => d.SubscriptionPlan).WithMany(p => p.PlanSettings)
                .HasForeignKey(d => d.SubscriptionPlanId)
                .HasConstraintName("FK__PlanSetti__Subsc__09A971A2");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Quiz__3214EC07459B7BE7");

            entity.ToTable("Quiz");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<QuizAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__QuizAnsw__3214EC07BF3B8EBD");

            entity.ToTable("QuizAnswer");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Question).WithMany(p => p.QuizAnswers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__QuizAnswe__Quest__0F624AF8");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__QuizQues__3214EC07F5493FC0");

            entity.ToTable("QuizQuestion");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK__QuizQuest__QuizI__0E6E26BF");
        });

        modelBuilder.Entity<QuizSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__QuizSett__3214EC07C96E3E6F");

            entity.ToTable("QuizSetting");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<RecurringTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Recurrin__3214EC075D4218D9");

            entity.ToTable("RecurringTransaction");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Amount).HasColumnType("decimal(15, 2)");

            entity.HasOne(d => d.Subcategory).WithMany(p => p.RecurringTransactions)
                .HasForeignKey(d => d.SubcategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Recurring__Subca__04E4BC85");

            entity.HasOne(d => d.User).WithMany(p => p.RecurringTransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Recurring__UserI__05D8E0BE");
        });

        modelBuilder.Entity<SpendingModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Spending__3214EC0701F304D5");

            entity.ToTable("SpendingModel");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.NameUnsign).HasMaxLength(50);
        });

        modelBuilder.Entity<SpendingModelCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Spending__3214EC07BAE87349");

            entity.ToTable("SpendingModelCategory");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PercentageAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.SpendingModelCategories)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__SpendingM__Categ__01142BA1");

            entity.HasOne(d => d.SpendingModel).WithMany(p => p.SpendingModelCategories)
                .HasForeignKey(d => d.SpendingModelId)
                .HasConstraintName("FK__SpendingM__Spend__00200768");
        });

        modelBuilder.Entity<Subcategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subcateg__3214EC0789F0B717");

            entity.ToTable("Subcategory");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.NameUnsign).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Subcategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subcatego__Categ__7F2BE32F");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC079370CD87");

            entity.ToTable("Subscription");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.SubscriptionPlan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.SubscriptionPlanId)
                .HasConstraintName("FK__Subscript__Subsc__06CD04F7");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Subscript__UserI__07C12930");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC07C2EDB1BA");

            entity.ToTable("SubscriptionPlan");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07AC448F93");

            entity.ToTable("Transaction");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Amount).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.ApprovalRequired).HasDefaultValue(false);

            entity.HasOne(d => d.Group).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Transacti__Group__797309D9");

            entity.HasOne(d => d.Subcategory).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.SubcategoryId)
                .HasConstraintName("FK__Transacti__Subca__7B5B524B");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Transacti__UserI__7A672E12");
        });

        modelBuilder.Entity<TransactionVote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC0761B10826");

            entity.ToTable("TransactionVote");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionVotes)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Trans__7C4F7684");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07E3373E1E");

            entity.ToTable("User");

            entity.HasIndex(e => e.PhoneNumber, "UQ__User__85FB4E38A29A5523").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534F6DE8EA9").IsUnique();

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.AvatarUrl);
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.GoogleId).HasMaxLength(200);
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.NameUnsign).HasMaxLength(50);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Address).HasMaxLength(255);
        });

        modelBuilder.Entity<UserQuizResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserQuiz__3214EC07650B7C4B");

            entity.ToTable("UserQuizResult");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.QuizData).IsRequired();

            entity.HasOne(d => d.Quiz).WithMany(p => p.UserQuizResults)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK__UserQuizR__QuizI__0D7A0286");

            entity.HasOne(d => d.User).WithMany(p => p.UserQuizResults)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserQuizR__UserI__0C85DE4D");
        });

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserSett__3214EC0752B59B0B");

            entity.ToTable("UserSetting");

            entity.HasIndex(e => e.UserSettingKey, "UQ__UserSett__DEE0F037A031DEF3").IsUnique();

            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UserSettingKey).HasMaxLength(255);
            entity.HasOne(d => d.User).WithMany(p => p.UserSettings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserSetting__UserId__12345678");
        });

        modelBuilder.Entity<UserSpendingModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserSpen__3214EC07E1F1D1C6");

            entity.ToTable("UserSpendingModel");

            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.SpendingModel).WithMany(p => p.UserSpendingModels)
                .HasForeignKey(d => d.SpendingModelId)
                .HasConstraintName("FK__UserSpend__Spend__02FC7413");

            entity.HasOne(d => d.User).WithMany(p => p.UserSpendingModels)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserSpend__UserI__02084FDA");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Image");
            entity.ToTable("Image");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}