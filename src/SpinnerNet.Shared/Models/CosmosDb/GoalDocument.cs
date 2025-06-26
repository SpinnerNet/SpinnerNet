// Removed System.Text.Json.Serialization - using direct property names for Cosmos DB (Microsoft NoSQL pattern)

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document for storing user goals with SMART criteria and ZeitCoin integration
/// Supports AI-generated insights, milestone tracking, and productivity optimization
/// Container: Goals, Partition Key: /UserId
/// </summary>
public class GoalDocument
{
    public string id { get; set; } = string.Empty;

    public string type { get; set; } = "goal";

    public string UserId { get; set; } = string.Empty;

    public string title { get; set; } = string.Empty;

    public string? description { get; set; }

    public string category { get; set; } = "Personal";

    public string priority { get; set; } = "Medium"; // Low, Medium, High, Critical

    public string status { get; set; } = "Active"; // Active, Completed, Paused, Cancelled

    public double progress { get; set; } = 0.0; // 0.0 to 1.0 (percentage)

    // SMART Criteria Properties
    public double? targetValue { get; set; }

    public string? targetUnit { get; set; }

    public DateTime? targetDate { get; set; }

    public int? estimatedHours { get; set; }

    public double? actualHours { get; set; }

    // ZeitCoin Integration
    public bool isZeitCoinEligible { get; set; } = true;

    public double zeitCoinMultiplier { get; set; } = 1.0;

    public int zeitCoinPointsEarned { get; set; } = 0;

    public int zeitCoinProjectedPoints { get; set; } = 0;

    // Organization
    public List<string> tags { get; set; } = new();

    public List<string> linkedTaskIds { get; set; } = new();

    public string? parentGoalId { get; set; }

    public List<string> childGoalIds { get; set; } = new();

    // AI Enhancement
    public bool aiInsightsEnabled { get; set; } = true;

    public int smartnessScore { get; set; } = 5; // 1-10 how SMART the goal is

    public int feasibilityScore { get; set; } = 7; // 1-10 how achievable

    public List<string> aiSuggestions { get; set; } = new();

    public List<string> riskFactors { get; set; } = new();

    // Tracking
    public DateTime? completedAt { get; set; }

    public DateTime? lastProgressUpdate { get; set; }

    public List<ProgressUpdate> progressUpdates { get; set; } = new();

    public List<Milestone> milestones { get; set; } = new();

    // Analytics
    public int viewCount { get; set; } = 0;

    public int editCount { get; set; } = 0;

    public int shareCount { get; set; } = 0;

    public int motivationLevel { get; set; } = 5; // 1-10 user-reported motivation

    // Standard fields
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    public DateTime updatedAt { get; set; } = DateTime.UtcNow;

    public int version { get; set; } = 1;

    public Dictionary<string, object> metadata { get; set; } = new();

    public bool isDeleted { get; set; } = false;

    // Cosmos DB required fields
    public string? ETag { get; set; }

    public int? ttl { get; set; }

}

/// <summary>
/// Progress update entry for goal tracking
/// </summary>
public class ProgressUpdate
{
    public DateTime timestamp { get; set; } = DateTime.UtcNow;

    public double progressValue { get; set; } // 0.0 to 1.0

    public string? notes { get; set; }

    public double? hoursWorked { get; set; }

    public int completedTasks { get; set; } = 0;

    public int zeitCoinPointsEarned { get; set; } = 0;

    public string? mood { get; set; } // Motivated, Frustrated, Confident, etc.

    public List<string> challenges { get; set; } = new();

    public List<string> wins { get; set; } = new();
}

/// <summary>
/// Milestone for goal breakdown and tracking
/// </summary>
public class Milestone
{
    public string id { get; set; } = Guid.NewGuid().ToString();

    public string title { get; set; } = string.Empty;

    public string? description { get; set; }

    public DateTime? targetDate { get; set; }

    public double? targetValue { get; set; }

    public string? targetUnit { get; set; }

    public bool isCompleted { get; set; } = false;

    public DateTime? completedAt { get; set; }

    public double progress { get; set; } = 0.0;

    public int zeitCoinPointsEarned { get; set; } = 0;

    public List<string> linkedTaskIds { get; set; } = new();

    public string? notes { get; set; }

    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    public DateTime updatedAt { get; set; } = DateTime.UtcNow;
}