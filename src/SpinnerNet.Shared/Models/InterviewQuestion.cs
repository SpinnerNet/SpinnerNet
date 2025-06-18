using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models;

/// <summary>
/// Represents a question in the AI persona interview process
/// </summary>
public class InterviewQuestion
{
    /// <summary>
    /// Unique identifier for the question
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The question text to display to the user
    /// </summary>
    [JsonPropertyName("questionText")]
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Type of question (text, multiple_choice, rating, etc.)
    /// </summary>
    [JsonPropertyName("questionType")]
    public string QuestionType { get; set; } = "text";

    /// <summary>
    /// Available options for multiple choice questions
    /// </summary>
    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();

    /// <summary>
    /// Whether this question is required
    /// </summary>
    [JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Category of information this question is trying to extract
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// AI context for generating follow-up questions
    /// </summary>
    [JsonPropertyName("aiContext")]
    public string? AiContext { get; set; }

    /// <summary>
    /// Additional context for the question
    /// </summary>
    [JsonPropertyName("context")]
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Expected type of response from user
    /// </summary>
    [JsonPropertyName("expectedResponseType")]
    public string ExpectedResponseType { get; set; } = "text";

    /// <summary>
    /// Suggestions or examples for the user
    /// </summary>
    [JsonPropertyName("suggestions")]
    public List<string> Suggestions { get; set; } = new();

    /// <summary>
    /// Whether this question is optional
    /// </summary>
    [JsonPropertyName("isOptional")]
    public bool IsOptional { get; set; } = false;

    /// <summary>
    /// The actual question text (alias for QuestionText for backward compatibility)
    /// </summary>
    [JsonPropertyName("question")]
    public string Question 
    { 
        get => QuestionText; 
        set => QuestionText = value; 
    }
}

/// <summary>
/// Metadata about user's response to help AI generate better follow-ups
/// </summary>
public class ResponseMetadata
{
    /// <summary>
    /// Time taken to respond (in seconds)
    /// </summary>
    [JsonPropertyName("responseTime")]
    public double ResponseTime { get; set; }

    /// <summary>
    /// Confidence level of the response (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; } = 1.0;

    /// <summary>
    /// Additional context about the response
    /// </summary>
    [JsonPropertyName("context")]
    public Dictionary<string, string> Context { get; set; } = new();
}

/// <summary>
/// Information extracted from user responses during interview
/// </summary>
public class ExtractedInfo
{
    /// <summary>
    /// User's interests and hobbies
    /// </summary>
    [JsonPropertyName("interests")]
    public List<string> Interests { get; set; } = new();

    /// <summary>
    /// User's goals and aspirations
    /// </summary>
    [JsonPropertyName("goals")]
    public List<string> Goals { get; set; } = new();

    /// <summary>
    /// User's communication style preferences
    /// </summary>
    [JsonPropertyName("communicationStyle")]
    public string CommunicationStyle { get; set; } = string.Empty;

    /// <summary>
    /// User's personality traits
    /// </summary>
    [JsonPropertyName("personalityTraits")]
    public Dictionary<string, double> PersonalityTraits { get; set; } = new();

    /// <summary>
    /// Cultural background information
    /// </summary>
    [JsonPropertyName("culturalContext")]
    public Dictionary<string, string> CulturalContext { get; set; } = new();

    /// <summary>
    /// User's occupation or profession
    /// </summary>
    [JsonPropertyName("occupation")]
    public string Occupation { get; set; } = string.Empty;

    /// <summary>
    /// User's age
    /// </summary>
    [JsonPropertyName("age")]
    public int? Age { get; set; }

    /// <summary>
    /// Languages the user speaks
    /// </summary>
    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = new();

    /// <summary>
    /// Cultural background information
    /// </summary>
    [JsonPropertyName("culturalBackground")]
    public string CulturalBackground { get; set; } = string.Empty;

    /// <summary>
    /// User interface preferences
    /// </summary>
    [JsonPropertyName("uiPreferences")]
    public Dictionary<string, object> UIPreferences { get; set; } = new();
}