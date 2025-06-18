using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document for storing user goals with SMART criteria and ZeitCoin integration
/// Supports AI-generated insights, milestone tracking, and productivity optimization
/// Container: Goals, Partition Key: /userId
/// </summary>
public class GoalDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "goal";

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = "Personal";

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Active"; // Active, Completed, Paused, Cancelled

    [JsonPropertyName("progress")]
    public double Progress { get; set; } = 0.0; // 0.0 to 1.0 (percentage)

    // SMART Criteria Properties
    [JsonPropertyName("targetValue")]
    public double? TargetValue { get; set; }

    [JsonPropertyName("targetUnit")]
    public string? TargetUnit { get; set; }

    [JsonPropertyName("targetDate")]
    public DateTime? TargetDate { get; set; }

    [JsonPropertyName("estimatedHours")]
    public int? EstimatedHours { get; set; }

    [JsonPropertyName("actualHours")]
    public double? ActualHours { get; set; }

    // ZeitCoin Integration
    [JsonPropertyName("isZeitCoinEligible")]
    public bool IsZeitCoinEligible { get; set; } = true;

    [JsonPropertyName("zeitCoinMultiplier")]
    public double ZeitCoinMultiplier { get; set; } = 1.0;

    [JsonPropertyName("zeitCoinPointsEarned")]
    public int ZeitCoinPointsEarned { get; set; } = 0;

    [JsonPropertyName("zeitCoinProjectedPoints")]
    public int ZeitCoinProjectedPoints { get; set; } = 0;

    // Organization
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("linkedTaskIds")]
    public List<string> LinkedTaskIds { get; set; } = new();

    [JsonPropertyName("parentGoalId")]
    public string? ParentGoalId { get; set; }

    [JsonPropertyName("childGoalIds")]
    public List<string> ChildGoalIds { get; set; } = new();

    // AI Enhancement
    [JsonPropertyName("aiInsightsEnabled")]
    public bool AiInsightsEnabled { get; set; } = true;

    [JsonPropertyName("smartnessScore")]
    public int SmartnessScore { get; set; } = 5; // 1-10 how SMART the goal is

    [JsonPropertyName("feasibilityScore")]
    public int FeasibilityScore { get; set; } = 7; // 1-10 how achievable

    [JsonPropertyName("aiSuggestions")]
    public List<string> AiSuggestions { get; set; } = new();

    [JsonPropertyName("riskFactors")]
    public List<string> RiskFactors { get; set; } = new();

    // Tracking
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("lastProgressUpdate")]
    public DateTime? LastProgressUpdate { get; set; }

    [JsonPropertyName("progressUpdates")]
    public List<ProgressUpdate> ProgressUpdates { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    // Analytics
    [JsonPropertyName("viewCount")]
    public int ViewCount { get; set; } = 0;

    [JsonPropertyName("editCount")]
    public int EditCount { get; set; } = 0;

    [JsonPropertyName("shareCount")]
    public int ShareCount { get; set; } = 0;

    [JsonPropertyName("motivationLevel")]
    public int MotivationLevel { get; set; } = 5; // 1-10 user-reported motivation

    // Standard fields
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    // Cosmos DB required fields
    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }

    [JsonPropertyName("ttl")]
    public int? TimeToLive { get; set; }

}

/// <summary>
/// Progress update entry for goal tracking
/// </summary>
public class ProgressUpdate
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("progressValue")]
    public double ProgressValue { get; set; } // 0.0 to 1.0

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("hoursWorked")]
    public double? HoursWorked { get; set; }

    [JsonPropertyName("completedTasks")]
    public int CompletedTasks { get; set; } = 0;

    [JsonPropertyName("zeitCoinPointsEarned")]
    public int ZeitCoinPointsEarned { get; set; } = 0;

    [JsonPropertyName("mood")]
    public string? Mood { get; set; } // Motivated, Frustrated, Confident, etc.

    [JsonPropertyName("challenges")]
    public List<string> Challenges { get; set; } = new();

    [JsonPropertyName("wins")]
    public List<string> Wins { get; set; } = new();
}

/// <summary>
/// Milestone for goal breakdown and tracking
/// </summary>
public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("targetDate")]
    public DateTime? TargetDate { get; set; }

    [JsonPropertyName("targetValue")]
    public double? TargetValue { get; set; }

    [JsonPropertyName("targetUnit")]
    public string? TargetUnit { get; set; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; } = false;

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("progress")]
    public double Progress { get; set; } = 0.0;

    [JsonPropertyName("zeitCoinPointsEarned")]
    public int ZeitCoinPointsEarned { get; set; } = 0;

    [JsonPropertyName("linkedTaskIds")]
    public List<string> LinkedTaskIds { get; set; } = new();

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}