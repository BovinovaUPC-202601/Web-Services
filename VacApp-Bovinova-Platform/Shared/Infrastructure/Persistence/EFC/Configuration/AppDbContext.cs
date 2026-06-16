using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Entities;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Entities;

namespace VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<BovineHealthRecord> BovineHealthRecords { get; set; }
    public DbSet<Collar> Collars { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<AdditionalCollarRequest> AdditionalCollarRequests { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<GeneralChatSession> GeneralChatSessions { get; set; }
    public DbSet<BovineChatSession> BovineChatSessions { get; set; }
    public DbSet<BovineAnalysis> BovineAnalyses { get; set; }
    public DbSet<BovineBreed> BovineBreeds { get; set; }
    public DbSet<CampaignStable> CampaignStables { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // DateOnly → DateTime
        configurationBuilder
            .Properties<DateOnly>()
            .HaveConversion<DateOnlyConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* IAM */
        //User
        builder.Entity<User>().HasKey(f => f.Id);
        builder.Entity<User>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<User>().Property(f => f.Username).IsRequired();
        builder.Entity<User>().Property(f => f.Password).IsRequired();
        builder.Entity<User>().Property(f => f.Email).IsRequired();
        builder.Entity<User>().Property(f => f.Role).IsRequired()
            .HasConversion<string>().HasColumnName("role");

        /* Ranch Management */
        //Stable
        builder.Entity<Stable>().HasKey(f => f.Id);
        builder.Entity<Stable>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Stable>().Property(f => f.Limit).IsRequired();
        builder.Entity<Stable>().Property(f => f.UserId).HasColumnName("user_id").IsRequired();

        //Bovine
        builder.Entity<Bovine>().HasKey(f => f.Id);
        builder.Entity<Bovine>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Bovine>().Property(f => f.Name).IsRequired();
        builder.Entity<Bovine>().Property(f => f.Gender).IsRequired();
        builder.Entity<Bovine>().Property(f => f.BirthDate).IsRequired();
        builder.Entity<Bovine>().Property(f => f.Breed).IsRequired();
        builder.Entity<Bovine>().Property(f => f.BovineImg).IsRequired();
        builder.Entity<Bovine>().Property(f => f.StableId).IsRequired();
        builder.Entity<Bovine>().Property(f => f.UserId).HasColumnName("user_id").IsRequired();
        builder.Entity<Bovine>().Property(f => f.MinTemperature).IsRequired();
        builder.Entity<Bovine>().Property(f => f.MaxTemperature).IsRequired();
        builder.Entity<Bovine>().Property(f => f.MinHeartRate).IsRequired();
        builder.Entity<Bovine>().Property(f => f.MaxHeartRate).IsRequired();

        //SEED BREEDS
        builder.Entity<BovineBreed>().HasData(
            new BovineBreed(1, "Holstein", 38.0, 39.3, 40, 80),
            new BovineBreed(2, "Jersey", 38.0, 39.4, 42, 82),
            new BovineBreed(3, "Brown Swiss", 37.9, 39.3, 39, 79),
            new BovineBreed(4, "Angus", 37.5, 39.0, 35, 75),
            new BovineBreed(5, "Hereford", 37.8, 39.2, 38, 78),
            new BovineBreed(6, "Charolais", 37.5, 39.1, 36, 76),
            new BovineBreed(7, "Limousin", 37.6, 39.1, 36, 76),
            new BovineBreed(8, "Wagyu", 37.5, 39.0, 35, 75),
            new BovineBreed(9, "Simmental", 37.8, 39.2, 38, 78),
            new BovineBreed(10, "Brahman", 38.5, 39.8, 45, 85),
            new BovineBreed(11, "Nelore", 38.4, 39.7, 44, 84),
            new BovineBreed(12, "Gyr", 38.3, 39.6, 43, 83),
            new BovineBreed(13, "Mestizo", 37.5, 39.6, 35, 85)
        );

        // Category
        builder.Entity<Category>().HasKey(f => f.Id);
        builder.Entity<Category>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Category>().Property(f => f.Name).IsRequired();
        builder.Entity<Category>().Property(f => f.UserId).IsRequired();

        // Product
        builder.Entity<Product>().HasKey(f => f.Id);
        builder.Entity<Product>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Product>().Property(f => f.Name).IsRequired();
        builder.Entity<Product>().Property(f => f.CategoryId).IsRequired();
        builder.Entity<Product>().Property(f => f.Quantity).IsRequired();
        builder.Entity<Product>().Property(f => f.UserId).IsRequired();
        builder.Entity<Product>().Property(f => f.ExpirationDate).IsRequired(false);
        builder.Entity<Product>().Property(f => f.Unit).IsRequired(false);
        builder.Entity<Product>()
            .HasOne(f => f.Category)
            .WithMany()
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        /* Staff Administration */
        //Staff
        builder.Entity<Staff>().HasKey(f => f.Id);
        builder.Entity<Staff>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Staff>().Property(f => f.Name).IsRequired();
        builder.Entity<Staff>()
            .OwnsOne(f => f.EmployeeStatus, navigationBuilder =>
            {
                navigationBuilder.WithOwner().HasForeignKey("Id");
                navigationBuilder.Property(f => f.Value)
                    .IsRequired()
                    .HasColumnName("employee_status");
            });
        builder.Entity<Staff>().Property(f => f.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<Staff>().Property(f => f.Email).IsRequired()
            .HasMaxLength(150).HasColumnName("email").HasDefaultValue("");
        builder.Entity<Staff>().Property(f => f.LinkedUserId).HasColumnName("linked_user_id");
        builder.Entity<Staff>().Property(f => f.AccessLevel).IsRequired()
            .HasConversion<int>().HasColumnName("access_level")
            .HasDefaultValue(StaffAccessLevel.ReadOnly);
        // One linked user per owner. MySQL exempts NULL linked_user_id rows from
        // uniqueness, so legacy staff (not linked) never collide. Owner+email
        // duplicates are validated at service level instead of a unique index,
        // because legacy rows share the '' default email per owner.
        builder.Entity<Staff>().HasIndex(f => new { f.UserId, f.LinkedUserId }).IsUnique();

        /* Campaign Management */
        builder.Entity<Campaign>().HasKey(c => c.Id);
        builder.Entity<Campaign>().Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Campaign>().Property(c => c.Name).IsRequired();
        builder.Entity<Campaign>().Property(c => c.Description).IsRequired();
        builder.Entity<Campaign>().Property(c => c.StartDate).IsRequired();
        builder.Entity<Campaign>().Property(c => c.EndDate).IsRequired();
        builder.Entity<Campaign>().Property(c => c.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<Campaign>().HasMany(c => c.CampaignStables)
            .WithOne()
            .HasForeignKey(cs => cs.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CampaignStable>().HasKey(cs => new { cs.CampaignId, cs.StableId });
        builder.Entity<CampaignStable>().Property(cs => cs.CampaignId).IsRequired().HasColumnName("campaign_id");
        builder.Entity<CampaignStable>().Property(cs => cs.StableId).IsRequired().HasColumnName("stable_id");

        /* IoT Monitoring */
        builder.Entity<BovineHealthRecord>().HasKey(r => r.Id);
        builder.Entity<BovineHealthRecord>().Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<BovineHealthRecord>().Property(r => r.BovineId).IsRequired().HasColumnName("bovine_id");
        builder.Entity<BovineHealthRecord>().Property(r => r.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<BovineHealthRecord>().Property(r => r.DeviceId).IsRequired().HasMaxLength(100);
        builder.Entity<BovineHealthRecord>().Property(r => r.Temperature).IsRequired();
        builder.Entity<BovineHealthRecord>().Property(r => r.HeartRate).IsRequired();
        builder.Entity<BovineHealthRecord>().Property(r => r.BatteryLevel).IsRequired().HasColumnName("battery_level");
        builder.Entity<BovineHealthRecord>().Property(r => r.IsAlert).IsRequired();
        builder.Entity<BovineHealthRecord>().Property(r => r.RecordedAt).IsRequired();
        builder.Entity<BovineHealthRecord>()
            .HasOne<Bovine>()
            .WithMany()
            .HasForeignKey(r => r.BovineId)
            .OnDelete(DeleteBehavior.Cascade);

        // Collar
        builder.Entity<Collar>().HasKey(c => c.Id);
        builder.Entity<Collar>().Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Collar>().Property(c => c.DeviceId).IsRequired().HasMaxLength(100).HasColumnName("device_id");
        builder.Entity<Collar>().Property(c => c.BovineId).IsRequired().HasColumnName("bovine_id");
        builder.Entity<Collar>().Property(c => c.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<Collar>().Property(c => c.RegisteredAt).IsRequired().HasColumnName("registered_at");
        builder.Entity<Collar>().Property(c => c.LifecycleStatus).IsRequired()
            .HasConversion<string>().HasColumnName("lifecycle_status");
        builder.Entity<Collar>().HasIndex(c => c.DeviceId).IsUnique();
        builder.Entity<Collar>()
            .HasOne<Bovine>()
            .WithMany()
            .HasForeignKey(c => c.BovineId)
            .OnDelete(DeleteBehavior.Cascade);

        /* Subscription Management */
        builder.Entity<Subscription>().HasKey(s => s.Id);
        builder.Entity<Subscription>().Property(s => s.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Subscription>().Property(s => s.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<Subscription>().Property(s => s.Plan).IsRequired()
            .HasConversion<string>().HasColumnName("plan");
        builder.Entity<Subscription>().Property(s => s.Status).IsRequired()
            .HasConversion<string>().HasColumnName("status");
        builder.Entity<Subscription>().Property(s => s.StartDate).HasColumnName("start_date");
        builder.Entity<Subscription>().Property(s => s.NextRenewal).HasColumnName("next_renewal");
        builder.Entity<Subscription>().HasIndex(s => s.UserId).IsUnique();

        // AdditionalCollarRequest
        builder.Entity<AdditionalCollarRequest>().HasKey(r => r.Id);
        builder.Entity<AdditionalCollarRequest>().Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<AdditionalCollarRequest>().Property(r => r.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<AdditionalCollarRequest>().Property(r => r.Status).IsRequired()
            .HasConversion<string>().HasColumnName("status");
        builder.Entity<AdditionalCollarRequest>().Property(r => r.MonthlyAmount).IsRequired().HasColumnName("monthly_amount");
        builder.Entity<AdditionalCollarRequest>().Property(r => r.RequestedAt).IsRequired().HasColumnName("requested_at");

        // Payment (gateway billing history)
        builder.Entity<Payment>().HasKey(p => p.Id);
        builder.Entity<Payment>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Payment>().Property(p => p.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<Payment>().Property(p => p.Concept).IsRequired()
            .HasConversion<string>().HasColumnName("concept");
        builder.Entity<Payment>().Property(p => p.Amount).IsRequired().HasColumnName("amount");
        builder.Entity<Payment>().Property(p => p.Currency).IsRequired().HasColumnName("currency");
        builder.Entity<Payment>().Property(p => p.Status).IsRequired()
            .HasConversion<string>().HasColumnName("status");
        builder.Entity<Payment>().Property(p => p.ProviderRef).HasColumnName("provider_ref");
        builder.Entity<Payment>().Property(p => p.IdempotencyKey).IsRequired().HasColumnName("idempotency_key");
        builder.Entity<Payment>().Property(p => p.PaidAt).HasColumnName("paid_at");
        builder.Entity<Payment>().HasIndex(p => p.IdempotencyKey).IsUnique();
        builder.Entity<Payment>().HasIndex(p => p.ProviderRef);

        /* Alert Management */
        builder.Entity<Alert>().HasKey(a => a.Id);
        builder.Entity<Alert>().Property(a => a.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Alert>().Property(a => a.BovineId).HasColumnName("bovine_id"); // nullable: account-level alerts (CollarReturn) have no bovine
        builder.Entity<Alert>().Property(a => a.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<Alert>().Property(a => a.AlertType).IsRequired()
            .HasConversion<string>().HasColumnName("alert_type");
        builder.Entity<Alert>().Property(a => a.UrgencyLevel).IsRequired()
            .HasConversion<string>().HasColumnName("urgency_level");
        builder.Entity<Alert>().Property(a => a.Status).IsRequired()
            .HasConversion<string>().HasColumnName("status");
        builder.Entity<Alert>().Property(a => a.Message).IsRequired().HasMaxLength(500);
        builder.Entity<Alert>().Property(a => a.CreatedAt).IsRequired().HasColumnName("created_at");

        /* AI Assistant */
        // General Chat Session
        builder.Entity<GeneralChatSession>().HasKey(s => s.Id);
        builder.Entity<GeneralChatSession>().Property(s => s.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<GeneralChatSession>().Property(s => s.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<GeneralChatSession>().Property(s => s.MessagesJson).IsRequired().HasColumnType("text");
        builder.Entity<GeneralChatSession>().Property(s => s.CreatedAt).IsRequired();
        builder.Entity<GeneralChatSession>().Property(s => s.UpdatedAt).IsRequired();

        // Bovine Chat Session
        builder.Entity<BovineChatSession>().HasKey(s => s.Id);
        builder.Entity<BovineChatSession>().Property(s => s.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<BovineChatSession>().Property(s => s.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<BovineChatSession>().Property(s => s.BovineId).IsRequired().HasColumnName("bovine_id");
        builder.Entity<BovineChatSession>().Property(s => s.MessagesJson).IsRequired().HasColumnType("text");
        builder.Entity<BovineChatSession>().Property(s => s.CreatedAt).IsRequired();
        builder.Entity<BovineChatSession>().Property(s => s.UpdatedAt).IsRequired();

        // Bovine Analysis
        builder.Entity<BovineAnalysis>().HasKey(a => a.Id);
        builder.Entity<BovineAnalysis>().Property(a => a.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<BovineAnalysis>().Property(a => a.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<BovineAnalysis>().Property(a => a.BovineId).IsRequired().HasColumnName("bovine_id");
        builder.Entity<BovineAnalysis>().Property(a => a.Score).IsRequired();
        builder.Entity<BovineAnalysis>().Property(a => a.VisibleIssues).IsRequired().HasColumnType("text");
        builder.Entity<BovineAnalysis>().Property(a => a.UrgencyLevel).IsRequired();
        builder.Entity<BovineAnalysis>().Property(a => a.Recommendation).IsRequired().HasColumnType("text");
        builder.Entity<BovineAnalysis>().Property(a => a.Confidence).IsRequired();
        builder.Entity<BovineAnalysis>().Property(a => a.CreatedAt).IsRequired();

        builder.UseSnakeCaseNamingConvention();
    }
}

public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter()
        : base(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d))
    { }
}
