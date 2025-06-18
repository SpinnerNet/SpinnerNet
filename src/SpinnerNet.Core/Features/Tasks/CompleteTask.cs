using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Tasks;

/// <summary>
/// Vertical slice for completing tasks with time tracking
/// Core Zeitsparkasse feature for ZeitCoin foundation - tracks time-to-value metrics
/// </summary>
public static class CompleteTask
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("taskId")]
        public string TaskId { get; init; } = string.Empty;

        [JsonPropertyName("completionNotes")]
        public string? CompletionNotes { get; init; }

        [JsonPropertyName("actualTimeSpentMinutes")]
        public int? ActualTimeSpentMinutes { get; init; }

        [JsonPropertyName("productivityRating")]
        public int ProductivityRating { get; init; } = 5; // 1-10 scale

        [JsonPropertyName("satisfactionRating")]
        public int SatisfactionRating { get; init; } = 5; // 1-10 scale

        [JsonPropertyName("completedAt")]
        public DateTime? CompletedAt { get; init; }

        [JsonPropertyName("valueCreated")]
        public string? ValueCreated { get; init; }

        [JsonPropertyName("learningsGained")]
        public string? LearningsGained { get; init; }
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("taskId")]
        public string? TaskId { get; init; }

        [JsonPropertyName("completedTask")]
        public TaskDocument? CompletedTask { get; init; }

        [JsonPropertyName("timeTrackingData")]
        public TimeTrackingAnalysis? TimeTrackingData { get; init; }

        [JsonPropertyName("zeitCoinEligibility")]
        public ZeitCoinEligibility? ZeitCoinEligibility { get; init; }

        [JsonPropertyName("productivityInsights")]
        public List<ProductivityInsight> ProductivityInsights { get; init; } = new();

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(string taskId, TaskDocument task, TimeTrackingAnalysis timeData, 
            ZeitCoinEligibility eligibility, List<ProductivityInsight> insights) =>
            new() 
            { 
                Success = true, 
                TaskId = taskId, 
                CompletedTask = task,
                TimeTrackingData = timeData,
                ZeitCoinEligibility = eligibility,
                ProductivityInsights = insights
            };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
            new() { Success = false, ValidationErrors = validationErrors };
    }

    // 3. Supporting Models
    public record TimeTrackingAnalysis
    {
        [JsonPropertyName("estimatedMinutes")]
        public int EstimatedMinutes { get; init; }

        [JsonPropertyName("actualMinutes")]
        public int ActualMinutes { get; init; }

        [JsonPropertyName("accuracyPercentage")]
        public double AccuracyPercentage { get; init; }

        [JsonPropertyName("efficiencyScore")]
        public double EfficiencyScore { get; init; }

        [JsonPropertyName("timeToValueRatio")]
        public double TimeToValueRatio { get; init; }

        [JsonPropertyName("baselineContribution")]
        public double BaselineContribution { get; init; }

        [JsonPropertyName("startedAt")]
        public DateTime StartedAt { get; init; }

        [JsonPropertyName("completedAt")]
        public DateTime CompletedAt { get; init; }

        [JsonPropertyName("totalDurationHours")]
        public double TotalDurationHours { get; init; }
    }

    public record ZeitCoinEligibility
    {
        [JsonPropertyName("isEligible")]
        public bool IsEligible { get; init; }

        [JsonPropertyName("basePoints")]
        public int BasePoints { get; init; }

        [JsonPropertyName("efficiencyBonus")]
        public int EfficiencyBonus { get; init; }

        [JsonPropertyName("valueBonus")]
        public int ValueBonus { get; init; }

        [JsonPropertyName("totalEarnedPoints")]
        public int TotalEarnedPoints { get; init; }

        [JsonPropertyName("eligibilityReasons")]
        public List<string> EligibilityReasons { get; init; } = new();

        [JsonPropertyName("futureOptimizations")]
        public List<string> FutureOptimizations { get; init; } = new();
    }

    public record ProductivityInsight
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("impact")]
        public string Impact { get; init; } = "medium"; // low, medium, high

        [JsonPropertyName("actionable")]
        public bool Actionable { get; init; } = true;

        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; init; } = new();
    }

    // 4. Validator (Input Validation)
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MaximumLength(100).WithMessage("User ID must not exceed 100 characters");

            RuleFor(x => x.TaskId)
                .NotEmpty().WithMessage("Task ID is required")
                .MaximumLength(100).WithMessage("Task ID must not exceed 100 characters");

            RuleFor(x => x.ActualTimeSpentMinutes)
                .GreaterThan(0).WithMessage("Actual time spent must be greater than 0")
                .LessThanOrEqualTo(1440).WithMessage("Actual time spent cannot exceed 24 hours (1440 minutes)")
                .When(x => x.ActualTimeSpentMinutes.HasValue);

            RuleFor(x => x.ProductivityRating)
                .InclusiveBetween(1, 10).WithMessage("Productivity rating must be between 1 and 10");

            RuleFor(x => x.SatisfactionRating)
                .InclusiveBetween(1, 10).WithMessage("Satisfaction rating must be between 1 and 10");

            RuleFor(x => x.CompletionNotes)
                .MaximumLength(1000).WithMessage("Completion notes must not exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.CompletionNotes));

            RuleFor(x => x.ValueCreated)
                .MaximumLength(500).WithMessage("Value created description must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ValueCreated));

            RuleFor(x => x.LearningsGained)
                .MaximumLength(500).WithMessage("Learnings gained description must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.LearningsGained));
        }
    }

    // 5. Handler (Business Logic + Data Access)
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICosmosRepository<TaskDocument> _taskRepository;
        private readonly ICosmosRepository<UserDocument> _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ICosmosRepository<TaskDocument> taskRepository,
            ICosmosRepository<UserDocument> userRepository,
            ILogger<Handler> logger)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Completing task {TaskId} for user {UserId}", request.TaskId, request.UserId);

                // 1. Verify user exists
                var users = await _userRepository.QueryAsync(
                    u => u.UserId == request.UserId,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (!users.Any())
                {
                    _logger.LogWarning("Task completion attempted for non-existent user: {UserId}", request.UserId);
                    return Result.Failure("User not found");
                }

                var user = users.First();

                // 2. Get the task to complete
                var tasks = await _taskRepository.QueryAsync(
                    t => t.TaskId == request.TaskId && t.UserId == request.UserId,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (!tasks.Any())
                {
                    _logger.LogWarning("Task completion attempted for non-existent task: {TaskId}", request.TaskId);
                    return Result.Failure("Task not found");
                }

                var task = tasks.First();

                // 3. Verify task can be completed
                if (task.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Completed)
                {
                    _logger.LogWarning("Task completion attempted for already completed task: {TaskId}", request.TaskId);
                    return Result.Failure("Task is already completed");
                }

                // 4. Calculate time tracking data
                var completedAt = request.CompletedAt ?? DateTime.UtcNow;
                var timeTrackingData = CalculateTimeTrackingAnalysis(task, request, completedAt);

                // 5. Update task with completion data
                UpdateTaskWithCompletionData(task, request, timeTrackingData, completedAt);

                // 6. Calculate ZeitCoin eligibility
                var zeitCoinEligibility = CalculateZeitCoinEligibility(task, timeTrackingData, user);

                // 7. Generate productivity insights
                var productivityInsights = GenerateProductivityInsights(task, timeTrackingData, user);

                // 8. Save updated task
                await _taskRepository.CreateOrUpdateAsync(task, request.UserId, cancellationToken);

                _logger.LogInformation("Task {TaskId} completed successfully. ZeitCoin eligible: {Eligible}, Points: {Points}", 
                    request.TaskId, zeitCoinEligibility.IsEligible, zeitCoinEligibility.TotalEarnedPoints);

                return Result.SuccessResult(request.TaskId, task, timeTrackingData, zeitCoinEligibility, productivityInsights);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task {TaskId} for user {UserId}", request.TaskId, request.UserId);
                return Result.Failure("An error occurred completing the task. Please try again.");
            }
        }

        private TimeTrackingAnalysis CalculateTimeTrackingAnalysis(TaskDocument task, Command request, DateTime completedAt)
        {
            var estimatedMinutes = task.EstimatedMinutes ?? 15; // Default estimate
            var actualMinutes = request.ActualTimeSpentMinutes ?? CalculateActualTimeFromDuration(task, completedAt);
            
            var accuracyPercentage = CalculateAccuracy(estimatedMinutes, actualMinutes);
            var efficiencyScore = CalculateEfficiencyScore(estimatedMinutes, actualMinutes, request.ProductivityRating);
            var timeToValueRatio = CalculateTimeToValueRatio(actualMinutes, request.SatisfactionRating, request.ValueCreated);
            var baselineContribution = CalculateBaselineContribution(task, actualMinutes);

            return new TimeTrackingAnalysis
            {
                EstimatedMinutes = estimatedMinutes,
                ActualMinutes = actualMinutes,
                AccuracyPercentage = accuracyPercentage,
                EfficiencyScore = efficiencyScore,
                TimeToValueRatio = timeToValueRatio,
                BaselineContribution = baselineContribution,
                StartedAt = task.CreatedAt,
                CompletedAt = completedAt,
                TotalDurationHours = (completedAt - task.CreatedAt).TotalHours
            };
        }

        private int CalculateActualTimeFromDuration(TaskDocument task, DateTime completedAt)
        {
            // If no actual time provided, estimate from creation to completion
            var duration = completedAt - task.CreatedAt;
            return Math.Max(1, (int)duration.TotalMinutes);
        }

        private double CalculateAccuracy(int estimated, int actual)
        {
            if (estimated == 0) return 0;
            var accuracy = 100.0 - (Math.Abs(estimated - actual) / (double)estimated * 100.0);
            return Math.Max(0, Math.Min(100, accuracy));
        }

        private double CalculateEfficiencyScore(int estimated, int actual, int productivityRating)
        {
            var timeEfficiency = estimated > 0 ? (double)estimated / actual : 1.0;
            var productivityFactor = productivityRating / 10.0;
            return Math.Min(10.0, timeEfficiency * productivityFactor * 5.0);
        }

        private double CalculateTimeToValueRatio(int actualMinutes, int satisfactionRating, string? valueCreated)
        {
            var baseRatio = satisfactionRating / 10.0;
            var valueBonus = !string.IsNullOrEmpty(valueCreated) ? 1.2 : 1.0;
            var timeNormalization = Math.Max(0.1, 1.0 - (actualMinutes / 1440.0)); // Favor shorter completion times
            
            return baseRatio * valueBonus * timeNormalization;
        }

        private double CalculateBaselineContribution(TaskDocument task, int actualMinutes)
        {
            // Higher contribution for tasks with better categories and priorities
            var categoryWeight = task.Category switch
            {
                "work" => 1.5,
                "health" => 1.3,
                "learning" => 1.4,
                "family" => 1.2,
                _ => 1.0
            };

            var priorityWeight = task.Priority switch
            {
                TaskPriority.Urgent => 1.3,
                TaskPriority.High => 1.2,
                TaskPriority.Medium => 1.0,
                TaskPriority.Low => 0.8,
                _ => 1.0
            };

            return categoryWeight * priorityWeight * Math.Log(Math.Max(1, actualMinutes));
        }

        private void UpdateTaskWithCompletionData(TaskDocument task, Command request, TimeTrackingAnalysis timeData, DateTime completedAt)
        {
            task.Status = SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Completed;
            task.CompletedAt = completedAt;
            task.UpdatedAt = DateTime.UtcNow;
            task.ActualMinutes = timeData.ActualMinutes;
            
            // Add completion metadata
            if (task.Metadata == null) task.Metadata = new Dictionary<string, object>();
            
            task.Metadata["completionNotes"] = request.CompletionNotes ?? "";
            task.Metadata["productivityRating"] = request.ProductivityRating;
            task.Metadata["satisfactionRating"] = request.SatisfactionRating;
            task.Metadata["valueCreated"] = request.ValueCreated ?? "";
            task.Metadata["learningsGained"] = request.LearningsGained ?? "";
            task.Metadata["timeAccuracy"] = timeData.AccuracyPercentage;
            task.Metadata["efficiencyScore"] = timeData.EfficiencyScore;
            task.Metadata["timeToValueRatio"] = timeData.TimeToValueRatio;
            task.Metadata["baselineContribution"] = timeData.BaselineContribution;
        }

        private ZeitCoinEligibility CalculateZeitCoinEligibility(TaskDocument task, TimeTrackingAnalysis timeData, UserDocument user)
        {
            var isEligible = task.IsZeitCoinEligible && 
                            timeData.EfficiencyScore >= 3.0 && 
                            timeData.TimeToValueRatio >= 0.3;

            var basePoints = CalculateBasePoints(task, timeData);
            var efficiencyBonus = CalculateEfficiencyBonus(timeData);
            var valueBonus = CalculateValueBonus(timeData, task);
            var totalPoints = isEligible ? basePoints + efficiencyBonus + valueBonus : 0;

            var eligibilityReasons = new List<string>();
            var optimizations = new List<string>();

            if (!isEligible)
            {
                if (!task.IsZeitCoinEligible) 
                    eligibilityReasons.Add("Task not marked as ZeitCoin eligible");
                if (timeData.EfficiencyScore < 3.0) 
                {
                    eligibilityReasons.Add($"Efficiency score too low: {timeData.EfficiencyScore:F1}/10");
                    optimizations.Add("Improve time estimation and focus during work");
                }
                if (timeData.TimeToValueRatio < 0.3) 
                {
                    eligibilityReasons.Add($"Time-to-value ratio too low: {timeData.TimeToValueRatio:F2}");
                    optimizations.Add("Focus on tasks that create more value or work more efficiently");
                }
            }
            else
            {
                eligibilityReasons.Add($"High efficiency score: {timeData.EfficiencyScore:F1}/10");
                eligibilityReasons.Add($"Good time-to-value ratio: {timeData.TimeToValueRatio:F2}");
                if (timeData.AccuracyPercentage > 80) 
                    eligibilityReasons.Add($"Excellent time estimation: {timeData.AccuracyPercentage:F1}%");
            }

            return new ZeitCoinEligibility
            {
                IsEligible = isEligible,
                BasePoints = basePoints,
                EfficiencyBonus = efficiencyBonus,
                ValueBonus = valueBonus,
                TotalEarnedPoints = totalPoints,
                EligibilityReasons = eligibilityReasons,
                FutureOptimizations = optimizations
            };
        }

        private int CalculateBasePoints(TaskDocument task, TimeTrackingAnalysis timeData)
        {
            var basePoints = Math.Max(1, timeData.ActualMinutes / 15); // 1 point per 15 minutes
            
            // Category bonuses
            var categoryMultiplier = task.Category switch
            {
                "work" => 1.5,
                "health" => 1.3,
                "learning" => 1.4,
                "family" => 1.2,
                _ => 1.0
            };

            // Priority bonuses
            var priorityMultiplier = task.Priority switch
            {
                TaskPriority.Urgent => 1.3,
                TaskPriority.High => 1.2,
                TaskPriority.Medium => 1.0,
                TaskPriority.Low => 0.8,
                _ => 1.0
            };

            return (int)(basePoints * categoryMultiplier * priorityMultiplier);
        }

        private int CalculateEfficiencyBonus(TimeTrackingAnalysis timeData)
        {
            if (timeData.EfficiencyScore >= 8.0) return 50;
            if (timeData.EfficiencyScore >= 6.0) return 25;
            if (timeData.EfficiencyScore >= 4.0) return 10;
            return 0;
        }

        private int CalculateValueBonus(TimeTrackingAnalysis timeData, TaskDocument task)
        {
            var valueBonus = 0;
            
            if (timeData.TimeToValueRatio >= 0.8) valueBonus += 30;
            else if (timeData.TimeToValueRatio >= 0.6) valueBonus += 15;
            else if (timeData.TimeToValueRatio >= 0.4) valueBonus += 5;

            if (timeData.AccuracyPercentage >= 90) valueBonus += 20;
            else if (timeData.AccuracyPercentage >= 80) valueBonus += 10;

            return valueBonus;
        }

        private List<ProductivityInsight> GenerateProductivityInsights(TaskDocument task, TimeTrackingAnalysis timeData, UserDocument user)
        {
            var insights = new List<ProductivityInsight>();

            // Time estimation insights
            if (timeData.AccuracyPercentage < 70)
            {
                var insight = new ProductivityInsight
                {
                    Type = "time_estimation",
                    Title = "Improve Time Estimation",
                    Description = $"Your time estimate was {timeData.AccuracyPercentage:F0}% accurate. Consider tracking similar tasks to improve estimation.",
                    Impact = "medium",
                    Data = new Dictionary<string, object>
                    {
                        {"estimated", timeData.EstimatedMinutes},
                        {"actual", timeData.ActualMinutes},
                        {"accuracy", timeData.AccuracyPercentage}
                    }
                };
                insights.Add(insight);
            }

            // Efficiency insights
            if (timeData.EfficiencyScore < 5.0)
            {
                insights.Add(new ProductivityInsight
                {
                    Type = "efficiency",
                    Title = "Focus on Efficiency",
                    Description = "Consider breaking large tasks into smaller chunks or eliminating distractions.",
                    Impact = "high",
                    Data = new Dictionary<string, object> { {"efficiencyScore", timeData.EfficiencyScore} }
                });
            }

            // Value creation insights
            if (timeData.TimeToValueRatio < 0.4)
            {
                insights.Add(new ProductivityInsight
                {
                    Type = "value_creation",
                    Title = "Maximize Value Creation",
                    Description = "Consider focusing on tasks that create more meaningful value or impact.",
                    Impact = "medium",
                    Data = new Dictionary<string, object> { {"timeToValueRatio", timeData.TimeToValueRatio} }
                });
            }

            // Pattern recognition insights
            if (task.Category == "work" && timeData.TotalDurationHours > 8)
            {
                insights.Add(new ProductivityInsight
                {
                    Type = "work_life_balance",
                    Title = "Work-Life Balance",
                    Description = "This work task took over 8 hours. Consider time-boxing and breaks.",
                    Impact = "high",
                    Data = new Dictionary<string, object> { {"duration", timeData.TotalDurationHours} }
                });
            }

            return insights;
        }
    }

    // 6. Endpoint (HTTP API)
    [ApiController]
    [Route("api/tasks")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Complete a task with time tracking and productivity analysis
        /// </summary>
        /// <param name="command">Task completion data</param>
        /// <returns>Completion result with ZeitCoin eligibility and insights</returns>
        [HttpPost("{taskId}/complete")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public async Task<ActionResult<Result>> CompleteTask([FromRoute] string taskId, [FromBody] Command command)
        {
            // Ensure taskId from route matches command
            var updatedCommand = command with { TaskId = taskId };
            
            var result = await _mediator.Send(updatedCommand);

            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(result);
                }

                if (result.ValidationErrors != null)
                {
                    return BadRequest(result);
                }

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}