/*
 * COMMENTED OUT FOR SPRINT 1 - USING COSMOS DB INSTEAD OF ENTITY FRAMEWORK
 * This Entity Framework DbContext is for future use when SQL database is needed
 * Currently using Cosmos DB for all data storage
 */

/*
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpinnerNet.Shared.Models;

namespace SpinnerNet.Core.Data;

/// <summary>
/// Main database context for the Spinner.Net application
/// </summary>
public class SpinnerNetDbContext : IdentityDbContext<ApplicationUser>
{
    public SpinnerNetDbContext(DbContextOptions<SpinnerNetDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Users in the platform
    /// </summary>
    public DbSet<User> SpinnerNetUsers { get; set; } = null!;

    /// <summary>
    /// User personas
    /// </summary>
    public DbSet<UserPersona> UserPersonas { get; set; } = null!;

    /// <summary>
    /// Tasks for time tracking and ZeitCoin earning
    /// </summary>
    public DbSet<SpinnerNet.Shared.Models.Task> Tasks { get; set; } = null!;

    /// <summary>
    /// Time entries for productivity tracking
    /// </summary>
    public DbSet<TimeEntry> TimeEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure User entity with enhanced Sprint 1 features
        builder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique(); // Ensure unique emails
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10).HasDefaultValue("en");
            entity.Property(e => e.CulturalBackground).HasMaxLength(100);
            entity.Property(e => e.TimeZone).HasMaxLength(50).HasDefaultValue("UTC");
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.DataResidencyPreference).HasMaxLength(50).HasDefaultValue("Local");
            entity.Property(e => e.DataSovereigntySettingsJson).HasDefaultValue("{}");
            entity.Property(e => e.SafetySettingsJson).HasDefaultValue("{}");
            entity.Property(e => e.ParentalEmail).HasMaxLength(256);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");
        });

        // Configure UserPersona entity
        builder.Entity<UserPersona>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.DigitalName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CulturalBackground).HasMaxLength(100);
            entity.Property(e => e.PrimaryLanguages).HasDefaultValue("[\"en\"]");
            entity.Property(e => e.PrimaryTimeZone).HasMaxLength(50).HasDefaultValue("UTC");
            entity.Property(e => e.PersonalityProfile).HasDefaultValue("{}");
            entity.Property(e => e.LearningStyle).HasMaxLength(50).HasDefaultValue("Visual");
            entity.Property(e => e.CommunicationPreferences).HasMaxLength(50).HasDefaultValue("Friendly");
            entity.Property(e => e.UIComplexityLevel).HasMaxLength(20).HasDefaultValue("Standard");
            entity.Property(e => e.ColorPreferences).HasMaxLength(50).HasDefaultValue("Default");
            entity.Property(e => e.FontSizePreferences).HasMaxLength(20).HasDefaultValue("Medium");
            entity.Property(e => e.NavigationStyle).HasMaxLength(20).HasDefaultValue("Standard");
            entity.Property(e => e.DataSovereigntyLevel).HasMaxLength(20).HasDefaultValue("Balanced");
            entity.Property(e => e.DefaultSharingProfile).HasMaxLength(20).HasDefaultValue("Selective");
            entity.Property(e => e.PreferredInteractionStyle).HasMaxLength(20).HasDefaultValue("Conversational");
            entity.Property(e => e.CompanionPersonality).HasMaxLength(50).HasDefaultValue("Helpful");
            entity.Property(e => e.DataRetentionPreference).HasDefaultValue(365);
            entity.Property(e => e.EnableAnimations).HasDefaultValue(true);
            entity.Property(e => e.RequireExplicitConsent).HasDefaultValue(true);
            entity.Property(e => e.EnableProactiveAssistance).HasDefaultValue(true);

            // Foreign key relationship
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Task entity
        builder.Entity<SpinnerNet.Shared.Models.Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("personal");
            entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("medium");
            entity.Property(e => e.Status).HasDefaultValue(SpinnerNet.Shared.Models.TaskStatus.Pending);
            entity.Property(e => e.IsZeitCoinEligible).HasDefaultValue(true);
            entity.Property(e => e.TagsJson).HasDefaultValue("[]");
            entity.Property(e => e.MetadataJson).HasDefaultValue("{}");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");

            // Foreign key relationship
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TimeEntry entity
        builder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).HasDefaultValue(TimeEntryType.Work);
            entity.Property(e => e.ProductivityLevel).HasDefaultValue(3);
            entity.Property(e => e.IsZeitCoinEligible).HasDefaultValue(true);
            entity.Property(e => e.TrackingMethod).HasMaxLength(20).HasDefaultValue("manual");
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");

            // Foreign key relationships
            entity.HasOne(e => e.Task)
                  .WithMany(t => t.TimeEntries)
                  .HasForeignKey(e => e.TaskId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed some default data
        SeedDefaultData(builder);
    }

    private void SeedDefaultData(ModelBuilder builder)
    {
        // Add any default data seeding here if needed
    }
}
*/