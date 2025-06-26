/*
 * COMMENTED OUT FOR SPRINT 1 SIMPLIFICATION - TASK FEATURES ARE FUTURE SPRINT
 * This entire Tasks feature will be implemented in a later sprint
 * Focus: User registration + Persona creation only for Sprint 1
 */

/*
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SpinnerNet.Core.Features.Tasks;

/// <summary>
/// Vertical slice for creating tasks from natural language input
/// Core Zeitsparkasse feature: "Remind me to call mom tomorrow" → structured task
/// </summary>
public static class CreateTask
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("personaId")]
        public string? PersonaId { get; init; }

        [JsonPropertyName("input")]
        public string Input { get; init; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; init; } = "en";

        [JsonPropertyName("timezone")]
        public string Timezone { get; init; } = "UTC";

        [JsonPropertyName("context")]
        public TaskContext? Context { get; init; }
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("taskId")]
        public string? TaskId { get; init; }

        [JsonPropertyName("task")]
        public TaskDocument? Task { get; init; }

        [JsonPropertyName("aiInsights")]
        public TaskAIInsights? AIInsights { get; init; }

        [JsonPropertyName("suggestedActions")]
        public List<SuggestedAction> SuggestedActions { get; init; } = new();

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(string taskId, TaskDocument task, TaskAIInsights insights, List<SuggestedAction> actions) =>
            new() 
            { 
                Success = true, 
                TaskId = taskId, 
                Task = task,
                AIInsights = insights,
                SuggestedActions = actions
            };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
            new() { Success = false, ValidationErrors = validationErrors };
    }

    // 3. Supporting Models
    public record TaskContext
    {
        [JsonPropertyName("currentLocation")]
        public string? CurrentLocation { get; init; }

        [JsonPropertyName("currentActivity")]
        public string? CurrentActivity { get; init; }

        [JsonPropertyName("relatedGoals")]
        public List<string> RelatedGoals { get; init; } = new();

        [JsonPropertyName("deviceType")]
        public string DeviceType { get; init; } = "web";
    }

    public record TaskAIInsights
    {
        [JsonPropertyName("extractedTitle")]
        public string ExtractedTitle { get; set; } = string.Empty;

        [JsonPropertyName("detectedDueDate")]
        public DateTime? DetectedDueDate { get; set; }

        [JsonPropertyName("detectedPriority")]
        public TaskPriority DetectedPriority { get; set; } = TaskPriority.Medium;

        [JsonPropertyName("suggestedCategory")]
        public string SuggestedCategory { get; set; } = "general";

        [JsonPropertyName("estimatedDuration")]
        public int? EstimatedDurationMinutes { get; set; }

        [JsonPropertyName("detectedEntities")]
        public List<string> DetectedEntities { get; set; } = new();

        [JsonPropertyName("suggestedTags")]
        public List<string> SuggestedTags { get; set; } = new();

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; } = 1.0;

        [JsonPropertyName("processingTime")]
        public TimeSpan ProcessingTime { get; set; }
    }

    public record SuggestedAction
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("actionData")]
        public Dictionary<string, object> ActionData { get; init; } = new();
    }

    // ... (all other content commented out for brevity in this example)
}

// Future reference for GetTask slice (placeholder)
public static class GetTask
{
    public static class Endpoint
    {
        public static string GetById => "GetTaskById";
    }
}
*/