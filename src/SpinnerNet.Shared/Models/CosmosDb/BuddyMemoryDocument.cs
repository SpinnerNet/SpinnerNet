using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing buddy memory and learning data
/// Container: BuddyMemory, Partition Key: /userId
/// </summary>
public class BuddyMemoryDocument
{
    /// <summary>
    /// Document ID (memory_${userId}_${memoryType}_${identifier})
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "buddyMemory";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Buddy identifier
    /// </summary>
    [JsonPropertyName("buddyId")]
    public string BuddyId { get; set; } = string.Empty;

    /// <summary>
    /// Type of memory (emailPatterns, interactionHistory, userPreferences, etc.)
    /// </summary>
    [JsonPropertyName("memoryType")]
    public string MemoryType { get; set; } = string.Empty;

    /// <summary>
    /// Memory data (structure varies by memory type)
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Confidence level of this memory (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; } = 0.5;

    /// <summary>
    /// When this memory was last updated
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this memory was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; set; }
}

/// <summary>
/// Specific memory type for email patterns
/// </summary>
public class EmailPatternsMemory
{
    /// <summary>
    /// Sender preferences and patterns
    /// </summary>
    [JsonPropertyName("senderPreferences")]
    public Dictionary<string, SenderPreference> SenderPreferences { get; set; } = new();

    /// <summary>
    /// Category patterns and keywords
    /// </summary>
    [JsonPropertyName("categoryPatterns")]
    public Dictionary<string, CategoryPattern> CategoryPatterns { get; set; } = new();

    /// <summary>
    /// User behavior patterns
    /// </summary>
    [JsonPropertyName("userBehaviorPatterns")]
    public UserBehaviorPatterns UserBehaviorPatterns { get; set; } = new();
}

/// <summary>
/// Preference data for a specific email sender
/// </summary>
public class SenderPreference
{
    /// <summary>
    /// Importance level for this sender (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("importance")]
    public double Importance { get; set; } = 0.5;

    /// <summary>
    /// Category this sender typically falls into
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = "unknown";

    /// <summary>
    /// Expected response time for this sender
    /// </summary>
    [JsonPropertyName("responseTimeExpected")]
    public string ResponseTimeExpected { get; set; } = "normal";

    /// <summary>
    /// VIP status
    /// </summary>
    [JsonPropertyName("isVip")]
    public bool IsVip { get; set; } = false;
}

/// <summary>
/// Pattern data for an email category
/// </summary>
public class CategoryPattern
{
    /// <summary>
    /// Keywords that indicate this category
    /// </summary>
    [JsonPropertyName("keywords")]
    public List<string> Keywords { get; set; } = new();

    /// <summary>
    /// Time patterns when these emails typically arrive
    /// </summary>
    [JsonPropertyName("timePatterns")]
    public List<string> TimePatterns { get; set; } = new();

    /// <summary>
    /// Indicators of urgency for this category
    /// </summary>
    [JsonPropertyName("urgencyIndicators")]
    public List<string> UrgencyIndicators { get; set; } = new();

    /// <summary>
    /// Common subject line patterns
    /// </summary>
    [JsonPropertyName("subjectPatterns")]
    public List<string> SubjectPatterns { get; set; } = new();
}

/// <summary>
/// User behavior patterns for email
/// </summary>
public class UserBehaviorPatterns
{
    /// <summary>
    /// Times when user typically reads emails
    /// </summary>
    [JsonPropertyName("readingTimes")]
    public List<string> ReadingTimes { get; set; } = new();

    /// <summary>
    /// Response patterns by email type
    /// </summary>
    [JsonPropertyName("responsePatterns")]
    public Dictionary<string, string> ResponsePatterns { get; set; } = new();

    /// <summary>
    /// User's typical email processing behavior
    /// </summary>
    [JsonPropertyName("processingBehavior")]
    public Dictionary<string, object> ProcessingBehavior { get; set; } = new();
}

/// <summary>
/// Specific memory type for interaction history
/// </summary>
public class InteractionHistoryMemory
{
    /// <summary>
    /// Recent interactions with the user
    /// </summary>
    [JsonPropertyName("recentInteractions")]
    public List<InteractionRecord> RecentInteractions { get; set; } = new();

    /// <summary>
    /// Conversation context and topics
    /// </summary>
    [JsonPropertyName("conversationContext")]
    public ConversationContext ConversationContext { get; set; } = new();

    /// <summary>
    /// User feedback on buddy responses
    /// </summary>
    [JsonPropertyName("userFeedback")]
    public List<FeedbackRecord> UserFeedback { get; set; } = new();
}

/// <summary>
/// Record of a single interaction
/// </summary>
public class InteractionRecord
{
    /// <summary>
    /// When the interaction occurred
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of interaction (chat, email_analysis, task_help, etc.)
    /// </summary>
    [JsonPropertyName("interactionType")]
    public string InteractionType { get; set; } = string.Empty;

    /// <summary>
    /// User's input or request
    /// </summary>
    [JsonPropertyName("userInput")]
    public string UserInput { get; set; } = string.Empty;

    /// <summary>
    /// Buddy's response
    /// </summary>
    [JsonPropertyName("buddyResponse")]
    public string BuddyResponse { get; set; } = string.Empty;

    /// <summary>
    /// Context or topic of the interaction
    /// </summary>
    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// User satisfaction with the interaction (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("userSatisfaction")]
    public double? UserSatisfaction { get; set; }
}

/// <summary>
/// Conversation context and ongoing topics
/// </summary>
public class ConversationContext
{
    /// <summary>
    /// Current active topics
    /// </summary>
    [JsonPropertyName("activeTopics")]
    public List<string> ActiveTopics { get; set; } = new();

    /// <summary>
    /// Recent context from conversations
    /// </summary>
    [JsonPropertyName("recentContext")]
    public List<ContextItem> RecentContext { get; set; } = new();

    /// <summary>
    /// Long-term context and user goals
    /// </summary>
    [JsonPropertyName("longTermContext")]
    public List<ContextItem> LongTermContext { get; set; } = new();
}

/// <summary>
/// A single context item
/// </summary>
public class ContextItem
{
    /// <summary>
    /// Context topic or subject
    /// </summary>
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Context details
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When this context was established
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Importance level (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("importance")]
    public double Importance { get; set; } = 0.5;
}

/// <summary>
/// User feedback on buddy performance
/// </summary>
public class FeedbackRecord
{
    /// <summary>
    /// When the feedback was given
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of feedback (rating, correction, suggestion, etc.)
    /// </summary>
    [JsonPropertyName("feedbackType")]
    public string FeedbackType { get; set; } = string.Empty;

    /// <summary>
    /// Feedback content
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Rating if applicable (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    /// <summary>
    /// What the feedback was about
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
}