// Removed System.Text.Json.Serialization - using direct property names for Cosmos DB (Microsoft NoSQL pattern)

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing buddy memory and learning data
/// Container: BuddyMemory, Partition Key: /UserId
/// </summary>
public class BuddyMemoryDocument
{
    /// <summary>
    /// Document ID (memory_${userId}_${memoryType}_${identifier})
    /// </summary>
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    public string type { get; set; } = "buddyMemory";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Buddy identifier
    /// </summary>
    public string buddyId { get; set; } = string.Empty;

    /// <summary>
    /// Type of memory (emailPatterns, interactionHistory, userPreferences, etc.)
    /// </summary>
    public string memoryType { get; set; } = string.Empty;

    /// <summary>
    /// Memory data (structure varies by memory type)
    /// </summary>
    public Dictionary<string, object> data { get; set; } = new();

    /// <summary>
    /// Confidence level of this memory (0.0 to 1.0)
    /// </summary>
    public double confidence { get; set; } = 0.5;

    /// <summary>
    /// When this memory was last updated
    /// </summary>
    public DateTime lastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this memory was created
    /// </summary>
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    public long _ts { get; set; }
}

/// <summary>
/// Specific memory type for email patterns
/// </summary>
public class EmailPatternsMemory
{
    /// <summary>
    /// Sender preferences and patterns
    /// </summary>
    public Dictionary<string, SenderPreference> senderPreferences { get; set; } = new();

    /// <summary>
    /// Category patterns and keywords
    /// </summary>
    public Dictionary<string, CategoryPattern> categoryPatterns { get; set; } = new();

    /// <summary>
    /// User behavior patterns
    /// </summary>
    public UserBehaviorPatterns userBehaviorPatterns { get; set; } = new();
}

/// <summary>
/// Preference data for a specific email sender
/// </summary>
public class SenderPreference
{
    /// <summary>
    /// Importance level for this sender (0.0 to 1.0)
    /// </summary>
    public double importance { get; set; } = 0.5;

    /// <summary>
    /// Category this sender typically falls into
    /// </summary>
    public string category { get; set; } = "unknown";

    /// <summary>
    /// Expected response time for this sender
    /// </summary>
    public string responseTimeExpected { get; set; } = "normal";

    /// <summary>
    /// VIP status
    /// </summary>
    public bool isVip { get; set; } = false;
}

/// <summary>
/// Pattern data for an email category
/// </summary>
public class CategoryPattern
{
    /// <summary>
    /// Keywords that indicate this category
    /// </summary>
    public List<string> keywords { get; set; } = new();

    /// <summary>
    /// Time patterns when these emails typically arrive
    /// </summary>
    public List<string> timePatterns { get; set; } = new();

    /// <summary>
    /// Indicators of urgency for this category
    /// </summary>
    public List<string> urgencyIndicators { get; set; } = new();

    /// <summary>
    /// Common subject line patterns
    /// </summary>
    public List<string> subjectPatterns { get; set; } = new();
}

/// <summary>
/// User behavior patterns for email
/// </summary>
public class UserBehaviorPatterns
{
    /// <summary>
    /// Times when user typically reads emails
    /// </summary>
    public List<string> readingTimes { get; set; } = new();

    /// <summary>
    /// Response patterns by email type
    /// </summary>
    public Dictionary<string, string> responsePatterns { get; set; } = new();

    /// <summary>
    /// User's typical email processing behavior
    /// </summary>
    public Dictionary<string, object> processingBehavior { get; set; } = new();
}

/// <summary>
/// Specific memory type for interaction history
/// </summary>
public class InteractionHistoryMemory
{
    /// <summary>
    /// Recent interactions with the user
    /// </summary>
    public List<InteractionRecord> recentInteractions { get; set; } = new();

    /// <summary>
    /// Conversation context and topics
    /// </summary>
    public ConversationContext conversationContext { get; set; } = new();

    /// <summary>
    /// User feedback on buddy responses
    /// </summary>
    public List<FeedbackRecord> userFeedback { get; set; } = new();
}

/// <summary>
/// Record of a single interaction
/// </summary>
public class InteractionRecord
{
    /// <summary>
    /// When the interaction occurred
    /// </summary>
    public DateTime timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of interaction (chat, email_analysis, task_help, etc.)
    /// </summary>
    public string interactionType { get; set; } = string.Empty;

    /// <summary>
    /// User's input or request
    /// </summary>
    public string userInput { get; set; } = string.Empty;

    /// <summary>
    /// Buddy's response
    /// </summary>
    public string buddyResponse { get; set; } = string.Empty;

    /// <summary>
    /// Context or topic of the interaction
    /// </summary>
    public string context { get; set; } = string.Empty;

    /// <summary>
    /// User satisfaction with the interaction (0.0 to 1.0)
    /// </summary>
    public double? userSatisfaction { get; set; }
}

/// <summary>
/// Conversation context and ongoing topics
/// </summary>
public class ConversationContext
{
    /// <summary>
    /// Current active topics
    /// </summary>
    public List<string> activeTopics { get; set; } = new();

    /// <summary>
    /// Recent context from conversations
    /// </summary>
    public List<ContextItem> recentContext { get; set; } = new();

    /// <summary>
    /// Long-term context and user goals
    /// </summary>
    public List<ContextItem> longTermContext { get; set; } = new();
}

/// <summary>
/// A single context item
/// </summary>
public class ContextItem
{
    /// <summary>
    /// Context topic or subject
    /// </summary>
    public string topic { get; set; } = string.Empty;

    /// <summary>
    /// Context details
    /// </summary>
    public string content { get; set; } = string.Empty;

    /// <summary>
    /// When this context was established
    /// </summary>
    public DateTime timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Importance level (0.0 to 1.0)
    /// </summary>
    public double importance { get; set; } = 0.5;
}

/// <summary>
/// User feedback on buddy performance
/// </summary>
public class FeedbackRecord
{
    /// <summary>
    /// When the feedback was given
    /// </summary>
    public DateTime timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of feedback (rating, correction, suggestion, etc.)
    /// </summary>
    public string feedbackType { get; set; } = string.Empty;

    /// <summary>
    /// Feedback content
    /// </summary>
    public string content { get; set; } = string.Empty;

    /// <summary>
    /// Rating if applicable (0.0 to 1.0)
    /// </summary>
    public double? rating { get; set; }

    /// <summary>
    /// What the feedback was about
    /// </summary>
    public string subject { get; set; } = string.Empty;
}