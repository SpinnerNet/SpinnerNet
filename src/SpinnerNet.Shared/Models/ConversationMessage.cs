using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models;

/// <summary>
/// Represents a single message in a conversation between user and AI buddy
/// </summary>
public class ConversationMessage
{
    /// <summary>
    /// Unique message identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the sender (user ID or buddy ID)
    /// </summary>
    [JsonPropertyName("senderId")]
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Type of sender (user, buddy)
    /// </summary>
    [JsonPropertyName("senderType")]
    public string SenderType { get; set; } = string.Empty;

    /// <summary>
    /// Message content
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the message was sent
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata (emotion, context, etc.)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}