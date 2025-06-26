// Removed System.Text.Json.Serialization - using direct property names for Cosmos DB (Microsoft NoSQL pattern)

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
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    public string type { get; set; } = "emailThread";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Email thread identifier
    /// </summary>
    public string threadId { get; set; } = string.Empty;

    /// <summary>
    /// Source email account information
    /// </summary>
    public EmailAccountInfo emailAccount { get; set; } = new();

    /// <summary>
    /// Thread metadata and summary information
    /// </summary>
    public EmailThreadInfo threadInfo { get; set; } = new();

    /// <summary>
    /// AI analysis of the email thread
    /// </summary>
    public EmailAIAnalysis aiAnalysis { get; set; } = new();

    /// <summary>
    /// Individual messages in the thread
    /// </summary>
    public List<EmailMessage> messages { get; set; } = new();

    /// <summary>
    /// When the thread was first created
    /// </summary>
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the thread was last updated
    /// </summary>
    public DateTime updatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    public long _ts { get; set; }
}

/// <summary>
/// Email account information
/// </summary>
public class EmailAccountInfo
{
    /// <summary>
    /// Email provider (gmail, outlook, etc.)
    /// </summary>
    public string provider { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string email { get; set; } = string.Empty;
}

/// <summary>
/// Email thread information and metadata
/// </summary>
public class EmailThreadInfo
{
    /// <summary>
    /// Email subject
    /// </summary>
    public string subject { get; set; } = string.Empty;

    /// <summary>
    /// Thread participants
    /// </summary>
    public List<EmailParticipant> participants { get; set; } = new();

    /// <summary>
    /// Number of messages in the thread
    /// </summary>
    public int messageCount { get; set; } = 0;

    /// <summary>
    /// Whether the thread has been read
    /// </summary>
    public bool isRead { get; set; } = false;

    /// <summary>
    /// Whether the thread has attachments
    /// </summary>
    public bool hasAttachments { get; set; } = false;
}

/// <summary>
/// Email thread participant
/// </summary>
public class EmailParticipant
{
    /// <summary>
    /// Participant's email address
    /// </summary>
    public string email { get; set; } = string.Empty;

    /// <summary>
    /// Participant's display name
    /// </summary>
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// Participant's role (sender, recipient, cc, bcc)
    /// </summary>
    public string role { get; set; } = string.Empty;
}

/// <summary>
/// AI analysis of email thread
/// </summary>
public class EmailAIAnalysis
{
    /// <summary>
    /// AI-determined category (work, personal, spam, promotional, etc.)
    /// </summary>
    public string category { get; set; } = "unknown";

    /// <summary>
    /// Priority score (0.0 to 1.0)
    /// </summary>
    public double priority { get; set; } = 0.5;

    /// <summary>
    /// Urgency score (0.0 to 1.0)
    /// </summary>
    public double urgency { get; set; } = 0.5;

    /// <summary>
    /// Sentiment analysis (positive, negative, neutral)
    /// </summary>
    public string sentiment { get; set; } = "neutral";

    /// <summary>
    /// AI-generated summary of the thread
    /// </summary>
    public string summary { get; set; } = string.Empty;

    /// <summary>
    /// Extracted action items
    /// </summary>
    public List<string> actionItems { get; set; } = new();

    /// <summary>
    /// When the analysis was performed
    /// </summary>
    public DateTime analyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Which buddy performed the analysis
    /// </summary>
    public string analyzedBy { get; set; } = string.Empty;
}

/// <summary>
/// Individual email message
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Message identifier
    /// </summary>
    public string messageId { get; set; } = string.Empty;

    /// <summary>
    /// Sender email address
    /// </summary>
    public string from { get; set; } = string.Empty;

    /// <summary>
    /// Recipient email addresses
    /// </summary>
    public List<string> to { get; set; } = new();

    /// <summary>
    /// CC email addresses
    /// </summary>
    public List<string> cc { get; set; } = new();

    /// <summary>
    /// BCC email addresses
    /// </summary>
    public List<string> bcc { get; set; } = new();

    /// <summary>
    /// Email subject
    /// </summary>
    public string subject { get; set; } = string.Empty;

    /// <summary>
    /// Preview of email body (first 200 characters)
    /// </summary>
    public string bodyPreview { get; set; } = string.Empty;

    /// <summary>
    /// Full email body (plain text)
    /// </summary>
    public string bodyText { get; set; } = string.Empty;

    /// <summary>
    /// HTML email body
    /// </summary>
    public string bodyHtml { get; set; } = string.Empty;

    /// <summary>
    /// When the message was received
    /// </summary>
    public DateTime receivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the message has been read
    /// </summary>
    public bool isRead { get; set; } = false;

    /// <summary>
    /// Message attachments
    /// </summary>
    public List<EmailAttachment> attachments { get; set; } = new();
}

/// <summary>
/// Email attachment information
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// Attachment filename
    /// </summary>
    public string filename { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long size { get; set; } = 0;

    /// <summary>
    /// MIME content type
    /// </summary>
    public string contentType { get; set; } = string.Empty;

    /// <summary>
    /// Attachment identifier for downloading
    /// </summary>
    public string attachmentId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the attachment is inline
    /// </summary>
    public bool isInline { get; set; } = false;
}