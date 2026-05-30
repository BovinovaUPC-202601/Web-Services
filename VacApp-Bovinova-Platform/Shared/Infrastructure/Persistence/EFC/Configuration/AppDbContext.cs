using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;

namespace VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<BovineHealthRecord> BovineHealthRecords { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<AISession> AISessions { get; set; }
    public DbSet<GeneralChatSession> GeneralChatSessions { get; set; }
    public DbSet<BovineChatSession> BovineChatSessions { get; set; }
    public DbSet<BovineAnalysis> BovineAnalyses { get; set; }

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

        /* Campaign Management */
        builder.Entity<Campaign>().HasKey(c => c.Id);
        builder.Entity<Campaign>().Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Campaign>().Property(c => c.Name).IsRequired();
        builder.Entity<Campaign>().Property(c => c.Description).IsRequired();
        builder.Entity<Campaign>().Property(c => c.StartDate).IsRequired();
        builder.Entity<Campaign>().Property(c => c.EndDate).IsRequired();
        builder.Entity<Campaign>().Property(c => c.UserId).IsRequired().HasColumnName("user_id");

        /* IoT Monitoring */
        builder.Entity<BovineHealthRecord>().HasKey(r => r.Id);
        builder.Entity<BovineHealthRecord>().Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<BovineHealthRecord>().Property(r => r.BovineId).IsRequired().HasColumnName("bovine_id");
        builder.Entity<BovineHealthRecord>().Property(r => r.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<BovineHealthRecord>().Property(r => r.DeviceId).IsRequired().HasMaxLength(100);
        builder.Entity<BovineHealthRecord>().Property(r => r.Temperature).IsRequired();
        builder.Entity<BovineHealthRecord>().Property(r => r.HeartRate).IsRequired();
        builder.Entity<BovineHealthRecord>().Property(r => r.IsAlert).IsRequired();
        builder.Entity<BovineHealthRecord>().Property(r => r.RecordedAt).IsRequired();
        builder.Entity<BovineHealthRecord>()
            .HasOne<Bovine>()
            .WithMany()
            .HasForeignKey(r => r.BovineId)
            .OnDelete(DeleteBehavior.Cascade);

        /* Alert Management */
        builder.Entity<Alert>().HasKey(a => a.Id);
        builder.Entity<Alert>().Property(a => a.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Alert>().Property(a => a.BovineId).IsRequired().HasColumnName("bovine_id");
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
        // AI Session
        builder.Entity<AISession>().HasKey(s => s.Id);
        builder.Entity<AISession>().Property(s => s.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<AISession>().Property(s => s.UserId).IsRequired().HasColumnName("user_id");
        builder.Entity<AISession>().Property(s => s.BovineId).HasColumnName("bovine_id");
        builder.Entity<AISession>().Property(s => s.SessionType).IsRequired();
        builder.Entity<AISession>().Property(s => s.CreatedAt).IsRequired();
        builder.Entity<AISession>().Property(s => s.UpdatedAt).IsRequired();

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
