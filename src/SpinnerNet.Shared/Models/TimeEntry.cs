using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpinnerNet.Shared.Models;

/// <summary>
/// Represents a time tracking entry for ZeitCoin calculation
/// Foundation for time banking and productivity measurement
/// </summary>
public class TimeEntry
{
    /// <summary>
    /// Unique identifier for the time entry
    /// </summary>
    [Key]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ID of the task this time entry belongs to
    /// </summary>
    [Required]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who logged this time
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// When the time tracking started
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// When the time tracking ended
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Duration in minutes (calculated or manual)
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Description of what was done during this time
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Type of time entry
    /// </summary>
    public TimeEntryType Type { get; set; } = TimeEntryType.Work;

    /// <summary>
    /// Productivity level during this time (1-5 scale)
    /// </summary>
    public int ProductivityLevel { get; set; } = 3;

    /// <summary>
    /// Whether this time entry is eligible for ZeitCoin earning
    /// </summary>
    public bool IsZeitCoinEligible { get; set; } = true;

    /// <summary>
    /// ZeitCoin amount earned for this time entry
    /// </summary>
    public decimal? ZeitCoinEarned { get; set; }

    /// <summary>
    /// How the time was tracked (manual, automatic, ai_assisted)
    /// </summary>
    [StringLength(20)]
    public string TrackingMethod { get; set; } = "manual";

    /// <summary>
    /// Whether this entry has been verified/validated
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// AI verification score (0.0 to 1.0)
    /// </summary>
    public double? VerificationScore { get; set; }

    /// <summary>
    /// When this time entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this time entry was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional notes about this time entry
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Foreign key to Task
    /// </summary>
    [ForeignKey(nameof(TaskId))]
    public Task Task { get; set; } = null!;

    /// <summary>
    /// Foreign key to User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    /// <summary>
    /// Calculate duration from start/end times
    /// </summary>
    public void CalculateDuration()
    {
        if (EndTime.HasValue)
        {
            DurationMinutes = (int)(EndTime.Value - StartTime).TotalMinutes;
        }
    }

    /// <summary>
    /// Check if this time entry is currently active (no end time)
    /// </summary>
    [NotMapped]
    public bool IsActive => !EndTime.HasValue;

    /// <summary>
    /// Get the effective duration (calculated or manual)
    /// </summary>
    [NotMapped]
    public int EffectiveDurationMinutes => EndTime.HasValue 
        ? (int)(EndTime.Value - StartTime).TotalMinutes 
        : DurationMinutes;
}

/// <summary>
/// Type of time entry for categorization
/// </summary>
public enum TimeEntryType
{
    /// <summary>
    /// Productive work time
    /// </summary>
    Work = 0,

    /// <summary>
    /// Learning and skill development
    /// </summary>
    Learning = 1,

    /// <summary>
    /// Creative activities
    /// </summary>
    Creative = 2,

    /// <summary>
    /// Planning and organization
    /// </summary>
    Planning = 3,

    /// <summary>
    /// Communication and collaboration
    /// </summary>
    Communication = 4,

    /// <summary>
    /// Problem solving and troubleshooting
    /// </summary>
    ProblemSolving = 5,

    /// <summary>
    /// Research and analysis
    /// </summary>
    Research = 6,

    /// <summary>
    /// Break time (typically not ZeitCoin eligible)
    /// </summary>
    Break = 7
}