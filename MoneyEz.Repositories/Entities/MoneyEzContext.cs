using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MoneyEz.Repositories.Entities;

public partial class MoneyEzContext : DbContext
{
    public MoneyEzContext()
    {
    }

    public MoneyEzContext(DbContextOptions<MoneyEzContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BudgetModel> BudgetModels { get; set; }

    public virtual DbSet<BudgetModelsSubcategory> BudgetModelsSubcategories { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryBudgetModel> CategoryBudgetModels { get; set; }

    public virtual DbSet<ChatHistory> ChatHistories { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<FixedTransaction> FixedTransactions { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<InquiryReport> InquiryReports { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PlanSetting> PlanSettings { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Subcategory> Subcategories { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<Target> Targets { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<UserSetting> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BudgetModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BudgetMo__3214EC076C0E3585");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired(true);

            entity.HasOne(d => d.Group).WithMany(p => p.BudgetModels)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_BudgetModels_Group");
        });

        modelBuilder.Entity<BudgetModelsSubcategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BudgetMo__3214EC0752210DD5");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("BudgetModelsSubcategory");

            entity.HasOne(d => d.BudgetModel).WithMany(p => p.BudgetModelsSubcategories)
                .HasForeignKey(d => d.BudgetModelId)
                .HasConstraintName("FK_BudgetModelsSubcategory_BudgetModel");

            entity.HasOne(d => d.Subcategory).WithMany(p => p.BudgetModelsSubcategories)
                .HasForeignKey(d => d.SubcategoryId)
                .HasConstraintName("FK_BudgetModelsSubcategory_Subcategory");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC076C8E8AF7");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconName).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired(true);
        });

        modelBuilder.Entity<CategoryBudgetModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07B20984CB");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.BudgetModel).WithMany(p => p.CategoryBudgetModels)
                .HasForeignKey(d => d.BudgetModelId)
                .HasConstraintName("FK_CategoryBudgetModels_BudgetModel");

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryBudgetModels)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_CategoryBudgetModels_Category");
        });

        modelBuilder.Entity<ChatHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatHist__3214EC07F429B993");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("ChatHistory");

            entity.Property(e => e.ConservationName).HasMaxLength(500);
            entity.Property(e => e.RoomNo).IsRequired(true);

            entity.HasOne(d => d.User).WithMany(p => p.ChatHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ChatHistory_User");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatMess__3214EC07F429B993");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("ChatMessage");

            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired(true);

            entity.HasOne(d => d.ChatHistory).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.ChatHistoryId)
                .HasConstraintName("FK_ChatMessage_ChatHistory");
        });

        modelBuilder.Entity<FixedTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FixedTra__3214EC070489ACAE");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("FixedTransaction");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired(true);

            entity.HasOne(d => d.Subcategory).WithMany(p => p.FixedTransactions)
                .HasForeignKey(d => d.SubcategoryId)
                .HasConstraintName("FK_FixedTransaction_Subcategory");

            entity.HasOne(d => d.User).WithMany(p => p.FixedTransactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_FixedTransaction_User");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Groups__3214EC078F1FD4AF");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(250).IsRequired(true);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Image");

            entity.Property(e => e.EntityName).HasMaxLength(100);

            entity.HasOne(d => d.Entity).WithMany()
                .HasForeignKey(d => d.EntityId)
                .HasConstraintName("FK_Image_Entity_Post");

            entity.HasOne(d => d.EntityNavigation).WithMany()
                .HasForeignKey(d => d.EntityId)
                .HasConstraintName("FK_Image_Entity");
        });

        modelBuilder.Entity<InquiryReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InquiryR__3214EC074754794F");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07ED702C28");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("Notification");

            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notification_User");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__3214EC0757225F8B");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentCode).HasMaxLength(250);
            entity.Property(e => e.TransactionCode).HasMaxLength(250);

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK_Payment_Subscription");
        });

        modelBuilder.Entity<PlanSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlanSett__3214EC0712224E4A");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("PlanSetting");

            entity.HasIndex(e => e.SettingKey, "UQ__PlanSett__01E719ADCCE98150").IsUnique();

            entity.Property(e => e.SettingKey).HasMaxLength(50).IsRequired(true);
            entity.Property(e => e.SettingValue).HasMaxLength(500).IsRequired(true);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Post__3214EC07A4C72A8A");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("Post");

            entity.Property(e => e.Title).HasMaxLength(500);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC07700B5FF1");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Subcategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subcateg__3214EC0753424064");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconName).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired(true);

            entity.HasOne(d => d.Category).WithMany(p => p.Subcategories)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Subcategories_Category");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC07D5942C0F");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("Subscription");

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK_Subscription_Plan");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Subscription_User");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC07BEFF8939");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("SubscriptionPlan");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired(true);
        });

        modelBuilder.Entity<Target>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Target__3214EC07EB826DA4");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("Target");

            entity.Property(e => e.Name).HasMaxLength(250).IsRequired(true);

            entity.HasOne(d => d.Group).WithMany(p => p.Targets)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_Target_Group");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07568FEEEB");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("Transaction");

            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(d => d.Group).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_Transaction_Group");

            entity.HasOne(d => d.Subcategory).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.SubcategoryId)
                .HasConstraintName("FK_Transaction_Subcategory");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Transaction_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC076F06BC40");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534501273D3").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(250).IsRequired(true);
            entity.Property(e => e.FullName).HasMaxLength(250).IsRequired(true);
            entity.Property(e => e.UnsignFullName).HasMaxLength(250);
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired(true);
            entity.Property(e => e.PhoneNumber).HasMaxLength(10);
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasNoKey();

            entity.HasOne(d => d.Group).WithMany()
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_UserGroups_Group");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserGroups_User");
        });

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserSett__3214EC0738361D04");
            entity.Property(x => x.Id).ValueGeneratedOnAdd();

            entity.ToTable("UserSetting");

            entity.HasIndex(e => e.SettingKey, "UQ__UserSett__01E719AD1D4FE8C8").IsUnique();

            entity.Property(e => e.SettingKey).HasMaxLength(50).IsRequired(true);
            entity.Property(e => e.SettingValue).HasMaxLength(500).IsRequired(true);

            entity.HasOne(d => d.User).WithMany(p => p.UserSettings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserSetting_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
