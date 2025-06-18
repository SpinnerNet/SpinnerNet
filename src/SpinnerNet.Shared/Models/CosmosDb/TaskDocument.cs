using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing a task in the Zeitsparkasse time management system
/// Container: Tasks, Partition Key: /userId
/// </summary>
public class TaskDocument
{
    /// <summary>
    /// Document ID (task_${userId}_${taskId})
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "task";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Task identifier
    /// </summary>
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Persona who created this task
    /// </summary>
    [JsonPropertyName("personaId")]
    public string? PersonaId { get; set; }

    /// <summary>
    /// Task title (AI-generated or user-provided)
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed task description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Original natural language input from user
    /// </summary>
    [JsonPropertyName("originalInput")]
    public string OriginalInput { get; set; } = string.Empty;

    /// <summary>
    /// Current task status
    /// </summary>
    [JsonPropertyName("status")]
    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    /// <summary>
    /// Task priority level
    /// </summary>
    [JsonPropertyName("priority")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// AI-determined task category
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = "general";

    /// <summary>
    /// Tags for organization and searching
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// When the task was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the task was last updated
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the task is due (optional)
    /// </summary>
    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// When the task was completed
    /// </summary>
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Estimated time to complete (in minutes)
    /// </summary>
    [JsonPropertyName("estimatedMinutes")]
    public int? EstimatedMinutes { get; set; }

    /// <summary>
    /// Actual time spent on task (in minutes)
    /// </summary>
    [JsonPropertyName("actualMinutes")]
    public int? ActualMinutes { get; set; }

    /// <summary>
    /// Goal this task is linked to
    /// </summary>
    [JsonPropertyName("goalId")]
    public string? GoalId { get; set; }

    /// <summary>
    /// Recurrence pattern for repeating tasks
    /// </summary>
    [JsonPropertyName("recurrence")]
    public TaskRecurrence? Recurrence { get; set; }

    /// <summary>
    /// Reminder settings
    /// </summary>
    [JsonPropertyName("reminders")]
    public List<TaskReminder> Reminders { get; set; } = new();

    /// <summary>
    /// AI-extracted context and insights
    /// </summary>
    [JsonPropertyName("aiContext")]
    public TaskAIContext AIContext { get; set; } = new();

    /// <summary>
    /// Collaboration information
    /// </summary>
    [JsonPropertyName("collaboration")]
    public TaskCollaboration? Collaboration { get; set; }

    /// <summary>
    /// Time tracking entries
    /// </summary>
    [JsonPropertyName("timeEntries")]
    public List<TimeEntry> TimeEntries { get; set; } = new();

    /// <summary>
    /// Additional metadata for task analytics and ZeitCoin tracking
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Whether this task is eligible for ZeitCoin rewards
    /// </summary>
    [JsonPropertyName("isZeitCoinEligible")]
    public bool IsZeitCoinEligible { get; set; } = true;

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; set; }
}

/// <summary>
/// Task status enumeration
/// </summary>
public enum TaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    Paused = 4,
    Waiting = 5
}

/// <summary>
/// Task priority levels
/// </summary>
public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}

/// <summary>
/// Task recurrence pattern
/// </summary>
public class TaskRecurrence
{
    [JsonPropertyName("pattern")]
    public RecurrencePattern Pattern { get; set; } = RecurrencePattern.None;

    [JsonPropertyName("interval")]
    public int Interval { get; set; } = 1;

    [JsonPropertyName("daysOfWeek")]
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("maxOccurrences")]
    public int? MaxOccurrences { get; set; }
}

/// <summary>
/// Recurrence patterns
/// </summary>
public enum RecurrencePattern
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4,
    Custom = 5
}

/// <summary>
/// Task reminder settings
/// </summary>
public class TaskReminder
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("triggerTime")]
    public DateTime TriggerTime { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public ReminderType Type { get; set; } = ReminderType.Notification;

    [JsonPropertyName("isTriggered")]
    public bool IsTriggered { get; set; } = false;
}

/// <summary>
/// Reminder types
/// </summary>
public enum ReminderType
{
    Notification = 0,
    Email = 1,
    SMS = 2,
    BuddyChat = 3
}

/// <summary>
/// AI-extracted context and insights for tasks
/// </summary>
public class TaskAIContext
{
    [JsonPropertyName("extractedEntities")]
    public List<string> ExtractedEntities { get; set; } = new();

    [JsonPropertyName("suggestedTags")]
    public List<string> SuggestedTags { get; set; } = new();

    [JsonPropertyName("detectedUrgency")]
    public double DetectedUrgency { get; set; } = 0.5;

    [JsonPropertyName("estimatedComplexity")]
    public string EstimatedComplexity { get; set; } = "medium";

    [JsonPropertyName("suggestedBreakdown")]
    public List<string> SuggestedBreakdown { get; set; } = new();

    [JsonPropertyName("relatedGoals")]
    public List<string> RelatedGoals { get; set; } = new();

    [JsonPropertyName("processingDate")]
    public DateTime ProcessingDate { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; } = 1.0;
}

/// <summary>
/// Task collaboration information
/// </summary>
public class TaskCollaboration
{
    [JsonPropertyName("sharedWith")]
    public List<string> SharedWith { get; set; } = new();

    [JsonPropertyName("assignedTo")]
    public string? AssignedTo { get; set; }

    [JsonPropertyName("permissions")]
    public Dictionary<string, string> Permissions { get; set; } = new();

    [JsonPropertyName("comments")]
    public List<TaskComment> Comments { get; set; } = new();
}

/// <summary>
/// Task comment for collaboration
/// </summary>
public class TaskComment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("editedAt")]
    public DateTime? EditedAt { get; set; }
}

/// <summary>
/// Time tracking entry
/// </summary>
public class TimeEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan? Duration { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("isAutoTracked")]
    public bool IsAutoTracked { get; set; } = false;

    [JsonPropertyName("productivity")]
    public double? Productivity { get; set; }
}