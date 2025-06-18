using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing a conversation between user and AI buddy
/// Container: Conversations, Partition Key: /userId
/// </summary>
public class ConversationDocument
{
    /// <summary>
    /// Document ID (conversation_${conversationId})
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "conversation";

    /// <summary>
    /// Conversation identifier
    /// </summary>
    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// AI buddy participating in this conversation
    /// </summary>
    [JsonPropertyName("buddyId")]
    public string BuddyId { get; set; } = string.Empty;

    /// <summary>
    /// When the conversation was started
    /// </summary>
    [JsonPropertyName("startedAt")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the last message was sent
    /// </summary>
    [JsonPropertyName("lastMessageAt")]
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the conversation is currently active
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Messages in this conversation
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ConversationMessage> Messages { get; set; } = new();

    /// <summary>
    /// Total number of messages in the conversation
    /// </summary>
    [JsonPropertyName("messageCount")]
    public int MessageCount { get; set; } = 0;

    /// <summary>
    /// Current topic or context of the conversation
    /// </summary>
    [JsonPropertyName("currentTopic")]
    public string? CurrentTopic { get; set; }

    /// <summary>
    /// Conversation metadata and analytics
    /// </summary>
    [JsonPropertyName("metadata")]
    public ConversationMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Individual message within a conversation
/// </summary>
public class ConversationMessage
{
    /// <summary>
    /// Unique message identifier
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Who sent the message (user or buddy)
    /// </summary>
    [JsonPropertyName("sender")]
    public MessageSender Sender { get; set; } = MessageSender.User;

    /// <summary>
    /// The message content
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Type of message (text, action, system, etc.)
    /// </summary>
    [JsonPropertyName("messageType")]
    public MessageType MessageType { get; set; } = MessageType.Text;

    /// <summary>
    /// When the message was sent
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Message metadata (emotion, confidence, etc.)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
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
    [JsonPropertyName("averageUserResponseTime")]
    public double AverageUserResponseTime { get; set; } = 0.0;

    /// <summary>
    /// Average response time from buddy (seconds)
    /// </summary>
    [JsonPropertyName("averageBuddyResponseTime")]
    public double AverageBuddyResponseTime { get; set; } = 0.0;

    /// <summary>
    /// Most frequently discussed topics
    /// </summary>
    [JsonPropertyName("frequentTopics")]
    public List<string> FrequentTopics { get; set; } = new();

    /// <summary>
    /// User satisfaction indicators
    /// </summary>
    [JsonPropertyName("satisfactionScore")]
    public double SatisfactionScore { get; set; } = 0.0;

    /// <summary>
    /// Number of tasks created through this conversation
    /// </summary>
    [JsonPropertyName("tasksCreated")]
    public int TasksCreated { get; set; } = 0;

    /// <summary>
    /// Conversation language
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";
}