using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing an AI persona interview session
/// Container: PersonaInterviews, Partition Key: /userId
/// </summary>
public class InterviewSessionDocument
{
    /// <summary>
    /// Document ID (interview_session_${sessionId})
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "interview_session";

    /// <summary>
    /// Interview session identifier
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the interview
    /// </summary>
    [JsonPropertyName("status")]
    public InterviewStatus Status { get; set; } = InterviewStatus.NotStarted;

    /// <summary>
    /// Interview progress tracking
    /// </summary>
    [JsonPropertyName("progress")]
    public InterviewProgress Progress { get; set; } = new();

    /// <summary>
    /// User responses collected so far
    /// </summary>
    [JsonPropertyName("responses")]
    public List<InterviewResponse> Responses { get; set; } = new();

    /// <summary>
    /// Extracted information from responses
    /// </summary>
    [JsonPropertyName("extractedInfo")]
    public ExtractedInfo ExtractedInfo { get; set; } = new();

    /// <summary>
    /// User's language preference for the interview
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";

    /// <summary>
    /// When the interview session was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the interview session was last updated
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the interview was completed (if applicable)
    /// </summary>
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Metadata about the interview session
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
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
    [JsonPropertyName("currentQuestionIndex")]
    public int CurrentQuestionIndex { get; set; } = 0;

    /// <summary>
    /// Total number of questions in the interview
    /// </summary>
    [JsonPropertyName("totalQuestions")]
    public int TotalQuestions { get; set; } = 0;

    /// <summary>
    /// Percentage completion (0-100)
    /// </summary>
    [JsonPropertyName("completionPercentage")]
    public double CompletionPercentage { get; set; } = 0.0;

    /// <summary>
    /// Estimated time remaining in minutes
    /// </summary>
    [JsonPropertyName("estimatedTimeRemainingMinutes")]
    public int EstimatedTimeRemainingMinutes { get; set; } = 0;

    /// <summary>
    /// Time spent on interview so far
    /// </summary>
    [JsonPropertyName("timeSpentMinutes")]
    public int TimeSpentMinutes { get; set; } = 0;

    /// <summary>
    /// Sections of the interview that have been completed
    /// </summary>
    [JsonPropertyName("completedSections")]
    public List<string> CompletedSections { get; set; } = new();

    /// <summary>
    /// Current section being worked on
    /// </summary>
    [JsonPropertyName("currentSection")]
    public string CurrentSection { get; set; } = "basics";
}

/// <summary>
/// Individual response within an interview session
/// </summary>
public class InterviewResponse
{
    /// <summary>
    /// ID of the question being answered
    /// </summary>
    [JsonPropertyName("questionId")]
    public string QuestionId { get; set; } = string.Empty;

    /// <summary>
    /// The user's response text
    /// </summary>
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// When this response was submitted
    /// </summary>
    [JsonPropertyName("answeredAt")]
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Time taken to answer this question (seconds)
    /// </summary>
    [JsonPropertyName("responseTime")]
    public double ResponseTime { get; set; } = 0.0;

    /// <summary>
    /// Confidence level in the response
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; } = 1.0;

    /// <summary>
    /// Additional metadata about the response
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}