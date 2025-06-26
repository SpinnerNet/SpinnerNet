// Removed System.Text.Json.Serialization - using direct property names for Cosmos DB (Microsoft NoSQL pattern)

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing an AI persona interview session
/// Container: PersonaInterviews, Partition Key: /UserId
/// </summary>
public class InterviewSessionDocument
{
    /// <summary>
    /// Document ID (interview_session_${sessionId})
    /// </summary>
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    public string type { get; set; } = "interview_session";

    /// <summary>
    /// Interview session identifier
    /// </summary>
    public string sessionId { get; set; } = string.Empty;

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the interview
    /// </summary>
    public InterviewStatus status { get; set; } = InterviewStatus.NotStarted;

    /// <summary>
    /// Interview progress tracking
    /// </summary>
    public InterviewProgress progress { get; set; } = new();

    /// <summary>
    /// User responses collected so far
    /// </summary>
    public List<InterviewResponse> responses { get; set; } = new();

    /// <summary>
    /// Extracted information from responses
    /// </summary>
    public ExtractedInfo extractedInfo { get; set; } = new();

    /// <summary>
    /// User's language preference for the interview
    /// </summary>
    public string language { get; set; } = "en";

    /// <summary>
    /// When the interview session was created
    /// </summary>
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the interview session was last updated
    /// </summary>
    public DateTime updatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the interview was completed (if applicable)
    /// </summary>
    public DateTime? completedAt { get; set; }

    /// <summary>
    /// Metadata about the interview session
    /// </summary>
    public Dictionary<string, object> metadata { get; set; } = new();
}

/// <summary>
/// Status of the persona interview
/// </summary>
public enum InterviewStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Abandoned = 3,
    Error = 4
}

/// <summary>
/// Interview progress tracking
/// </summary>
public class InterviewProgress
{
    /// <summary>
    /// Current question index
    /// </summary>
    public int currentQuestionIndex { get; set; } = 0;

    /// <summary>
    /// Total number of questions in the interview
    /// </summary>
    public int totalQuestions { get; set; } = 0;

    /// <summary>
    /// Percentage completion (0-100)
    /// </summary>
    public double completionPercentage { get; set; } = 0.0;

    /// <summary>
    /// Estimated time remaining in minutes
    /// </summary>
    public int estimatedTimeRemainingMinutes { get; set; } = 0;

    /// <summary>
    /// Time spent on interview so far
    /// </summary>
    public int timeSpentMinutes { get; set; } = 0;

    /// <summary>
    /// Sections of the interview that have been completed
    /// </summary>
    public List<string> completedSections { get; set; } = new();

    /// <summary>
    /// Current section being worked on
    /// </summary>
    public string currentSection { get; set; } = "basics";
}

/// <summary>
/// Individual response within an interview session
/// </summary>
public class InterviewResponse
{
    /// <summary>
    /// ID of the question being answered
    /// </summary>
    public string questionId { get; set; } = string.Empty;

    /// <summary>
    /// The user's response text
    /// </summary>
    public string response { get; set; } = string.Empty;

    /// <summary>
    /// When this response was submitted
    /// </summary>
    public DateTime answeredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Time taken to answer this question (seconds)
    /// </summary>
    public double responseTime { get; set; } = 0.0;

    /// <summary>
    /// Confidence level in the response
    /// </summary>
    public double confidence { get; set; } = 1.0;

    /// <summary>
    /// Additional metadata about the response
    /// </summary>
    public Dictionary<string, object> metadata { get; set; } = new();
}