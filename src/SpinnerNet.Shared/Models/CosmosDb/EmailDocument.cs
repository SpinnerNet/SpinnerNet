using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing an email thread with AI analysis
/// Container: EmailData, Partition Key: /userId
/// </summary>
public class EmailThreadDocument
{
    /// <summary>
    /// Document ID (email_${userId}_thread_${threadId})
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "emailThread";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Email thread identifier
    /// </summary>
    [JsonPropertyName("threadId")]
    public string ThreadId { get; set; } = string.Empty;

    /// <summary>
    /// Source email account information
    /// </summary>
    [JsonPropertyName("emailAccount")]
    public EmailAccountInfo EmailAccount { get; set; } = new();

    /// <summary>
    /// Thread metadata and summary information
    /// </summary>
    [JsonPropertyName("threadInfo")]
    public EmailThreadInfo ThreadInfo { get; set; } = new();

    /// <summary>
    /// AI analysis of the email thread
    /// </summary>
    [JsonPropertyName("aiAnalysis")]
    public EmailAIAnalysis AIAnalysis { get; set; } = new();

    /// <summary>
    /// Individual messages in the thread
    /// </summary>
    [JsonPropertyName("messages")]
    public List<EmailMessage> Messages { get; set; } = new();

    /// <summary>
    /// When the thread was first created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the thread was last updated
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; set; }
}

/// <summary>
/// Email account information
/// </summary>
public class EmailAccountInfo
{
    /// <summary>
    /// Email provider (gmail, outlook, etc.)
    /// </summary>
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Email thread information and metadata
/// </summary>
public class EmailThreadInfo
{
    /// <summary>
    /// Email subject
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Thread participants
    /// </summary>
    [JsonPropertyName("participants")]
    public List<EmailParticipant> Participants { get; set; } = new();

    /// <summary>
    /// Number of messages in the thread
    /// </summary>
    [JsonPropertyName("messageCount")]
    public int MessageCount { get; set; } = 0;

    /// <summary>
    /// Whether the thread has been read
    /// </summary>
    [JsonPropertyName("isRead")]
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Whether the thread has attachments
    /// </summary>
    [JsonPropertyName("hasAttachments")]
    public bool HasAttachments { get; set; } = false;
}

/// <summary>
/// Email thread participant
/// </summary>
public class EmailParticipant
{
    /// <summary>
    /// Participant's email address
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Participant's display name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Participant's role (sender, recipient, cc, bcc)
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// AI analysis of email thread
/// </summary>
public class EmailAIAnalysis
{
    /// <summary>
    /// AI-determined category (work, personal, spam, promotional, etc.)
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = "unknown";

    /// <summary>
    /// Priority score (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("priority")]
    public double Priority { get; set; } = 0.5;

    /// <summary>
    /// Urgency score (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("urgency")]
    public double Urgency { get; set; } = 0.5;

    /// <summary>
    /// Sentiment analysis (positive, negative, neutral)
    /// </summary>
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = "neutral";

    /// <summary>
    /// AI-generated summary of the thread
    /// </summary>
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Extracted action items
    /// </summary>
    [JsonPropertyName("actionItems")]
    public List<string> ActionItems { get; set; } = new();

    /// <summary>
    /// When the analysis was performed
    /// </summary>
    [JsonPropertyName("analyzedAt")]
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Which buddy performed the analysis
    /// </summary>
    [JsonPropertyName("analyzedBy")]
    public string AnalyzedBy { get; set; } = string.Empty;
}

/// <summary>
/// Individual email message
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Message identifier
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Sender email address
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Recipient email addresses
    /// </summary>
    [JsonPropertyName("to")]
    public List<string> To { get; set; } = new();

    /// <summary>
    /// CC email addresses
    /// </summary>
    [JsonPropertyName("cc")]
    public List<string> CC { get; set; } = new();

    /// <summary>
    /// BCC email addresses
    /// </summary>
    [JsonPropertyName("bcc")]
    public List<string> BCC { get; set; } = new();

    /// <summary>
    /// Email subject
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Preview of email body (first 200 characters)
    /// </summary>
    [JsonPropertyName("bodyPreview")]
    public string BodyPreview { get; set; } = string.Empty;

    /// <summary>
    /// Full email body (plain text)
    /// </summary>
    [JsonPropertyName("bodyText")]
    public string BodyText { get; set; } = string.Empty;

    /// <summary>
    /// HTML email body
    /// </summary>
    [JsonPropertyName("bodyHtml")]
    public string BodyHtml { get; set; } = string.Empty;

    /// <summary>
    /// When the message was received
    /// </summary>
    [JsonPropertyName("receivedAt")]
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the message has been read
    /// </summary>
    [JsonPropertyName("isRead")]
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Message attachments
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<EmailAttachment> Attachments { get; set; } = new();
}

/// <summary>
/// Email attachment information
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// Attachment filename
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; } = 0;

    /// <summary>
    /// MIME content type
    /// </summary>
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Attachment identifier for downloading
    /// </summary>
    [JsonPropertyName("attachmentId")]
    public string AttachmentId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the attachment is inline
    /// </summary>
    [JsonPropertyName("isInline")]
    public bool IsInline { get; set; } = false;
}