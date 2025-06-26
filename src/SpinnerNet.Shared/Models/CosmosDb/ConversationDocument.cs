// Removed System.Text.Json.Serialization - using direct property names for Cosmos DB (Microsoft NoSQL pattern)

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing a conversation between user and AI buddy
/// Container: Conversations, Partition Key: /UserId
/// </summary>
public class ConversationDocument
{
    /// <summary>
    /// Document ID (conversation_${conversationId})
    /// </summary>
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    public string type { get; set; } = "conversation";

    /// <summary>
    /// Conversation identifier
    /// </summary>
    public string conversationId { get; set; } = string.Empty;

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// AI buddy participating in this conversation
    /// </summary>
    public string buddyId { get; set; } = string.Empty;

    /// <summary>
    /// When the conversation was started
    /// </summary>
    public DateTime startedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the last message was sent
    /// </summary>
    public DateTime lastMessageAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the conversation is currently active
    /// </summary>
    public bool isActive { get; set; } = true;

    /// <summary>
    /// Messages in this conversation
    /// </summary>
    public List<ConversationMessage> messages { get; set; } = new();

    /// <summary>
    /// Total number of messages in the conversation
    /// </summary>
    public int messageCount { get; set; } = 0;

    /// <summary>
    /// Current topic or context of the conversation
    /// </summary>
    public string? currentTopic { get; set; }

    /// <summary>
    /// Conversation metadata and analytics
    /// </summary>
    public ConversationMetadata metadata { get; set; } = new();
}

/// <summary>
/// Individual message within a conversation
/// </summary>
public class ConversationMessage
{
    /// <summary>
    /// Unique message identifier
    /// </summary>
    public string messageId { get; set; } = string.Empty;

    /// <summary>
    /// Who sent the message (user or buddy)
    /// </summary>
    public MessageSender sender { get; set; } = MessageSender.User;

    /// <summary>
    /// The message content
    /// </summary>
    public string content { get; set; } = string.Empty;

    /// <summary>
    /// Type of message (text, action, system, etc.)
    /// </summary>
    public MessageType messageType { get; set; } = MessageType.Text;

    /// <summary>
    /// When the message was sent
    /// </summary>
    public DateTime timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Message metadata (emotion, confidence, etc.)
    /// </summary>
    public Dictionary<string, object> metadata { get; set; } = new();
}

/// <summary>
/// Who sent a message
/// </summary>
public enum MessageSender
{
    User = 0,
    Buddy = 1,
    System = 2
}

/// <summary>
/// Type of message content
/// </summary>
public enum MessageType
{
    Text = 0,
    Action = 1,
    System = 2,
    Greeting = 3,
    TaskCreated = 4,
    Suggestion = 5
}

/// <summary>
/// Response type for buddy messages
/// </summary>
public enum ResponseType
{
    Conversational = 0,
    TaskCreated = 1,
    Informational = 2,
    Suggestion = 3,
    Error = 4
}

/// <summary>
/// Conversation analytics and metadata
/// </summary>
public class ConversationMetadata
{
    /// <summary>
    /// Average response time from user (seconds)
    /// </summary>
    public double averageUserResponseTime { get; set; } = 0.0;

    /// <summary>
    /// Average response time from buddy (seconds)
    /// </summary>
    public double averageBuddyResponseTime { get; set; } = 0.0;

    /// <summary>
    /// Most frequently discussed topics
    /// </summary>
    public List<string> frequentTopics { get; set; } = new();

    /// <summary>
    /// User satisfaction indicators
    /// </summary>
    public double satisfactionScore { get; set; } = 0.0;

    /// <summary>
    /// Number of tasks created through this conversation
    /// </summary>
    public int tasksCreated { get; set; } = 0;

    /// <summary>
    /// Conversation language
    /// </summary>
    public string language { get; set; } = "en";
}