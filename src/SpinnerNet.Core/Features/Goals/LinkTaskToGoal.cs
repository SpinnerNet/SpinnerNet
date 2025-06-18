using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Goals;

/// <summary>
/// Vertical slice for linking tasks to goals with automatic progress calculation
/// Implements: Command → Validation → Handler → Endpoint pattern
/// Enables goal-driven task completion and optimized ZeitCoin rewards
/// </summary>
public static class LinkTaskToGoal
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("taskId")]
        public string TaskId { get; init; } = string.Empty;

        [JsonPropertyName("goalId")]
        public string GoalId { get; init; } = string.Empty;

        [JsonPropertyName("contributionWeight")]
        public double ContributionWeight { get; init; } = 1.0; // How much this task contributes to goal (0.1 to 1.0)

        [JsonPropertyName("milestoneId")]
        public string? MilestoneId { get; init; } // Optional: link to specific milestone

        [JsonPropertyName("expectedProgressImpact")]
        public double? ExpectedProgressImpact { get; init; } // How much goal progress this task should add

        [JsonPropertyName("zeitCoinBonusMultiplier")]
        public double ZeitCoinBonusMultiplier { get; init; } = 1.0; // Additional ZeitCoin multiplier for goal alignment

        [JsonPropertyName("notes")]
        public string? Notes { get; init; }

        [JsonPropertyName("autoCalculateProgress")]
        public bool AutoCalculateProgress { get; init; } = true;
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("linkId")]
        public string? LinkId { get; init; }

        [JsonPropertyName("goalProgress")]
        public GoalProgressInfo? GoalProgress { get; init; }

        [JsonPropertyName("zeitCoinImpact")]
        public ZeitCoinImpact? ZeitCoinImpact { get; init; }

        [JsonPropertyName("progressPrediction")]
        public ProgressPrediction? ProgressPrediction { get; init; }

        [JsonPropertyName("recommendedActions")]
        public List<string> RecommendedActions { get; init; } = new();

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(string linkId, GoalProgressInfo progress, 
            ZeitCoinImpact? zeitCoin = null, ProgressPrediction? prediction = null, 
            List<string>? actions = null) =>
            new()
            {
                Success = true,
                LinkId = linkId,
                GoalProgress = progress,
                ZeitCoinImpact = zeitCoin,
                ProgressPrediction = prediction,
                RecommendedActions = actions ?? new()
            };

        public static Result FailureResult(string error, Dictionary<string, string[]>? validationErrors = null) =>
            new()
            {
                Success = false,
                ErrorMessage = error,
                ValidationErrors = validationErrors
            };
    }

    // 3. Supporting Types
    public record GoalProgressInfo
    {
        [JsonPropertyName("goalId")]
        public string GoalId { get; init; } = string.Empty;

        [JsonPropertyName("currentProgress")]
        public double CurrentProgress { get; init; }

        [JsonPropertyName("previousProgress")]
        public double PreviousProgress { get; init; }

        [JsonPropertyName("progressIncrease")]
        public double ProgressIncrease { get; init; }

        [JsonPropertyName("totalLinkedTasks")]
        public int TotalLinkedTasks { get; init; }

        [JsonPropertyName("completedLinkedTasks")]
        public int CompletedLinkedTasks { get; init; }

        [JsonPropertyName("estimatedCompletionDate")]
        public DateTime? EstimatedCompletionDate { get; init; }

        [JsonPropertyName("isOnTrack")]
        public bool IsOnTrack { get; init; }

        [JsonPropertyName("daysAheadBehind")]
        public int DaysAheadBehind { get; init; }
    }

    public record ZeitCoinImpact
    {
        [JsonPropertyName("bonusMultiplier")]
        public double BonusMultiplier { get; init; }

        [JsonPropertyName("projectedBonusPoints")]
        public int ProjectedBonusPoints { get; init; }

        [JsonPropertyName("goalAlignmentBonus")]
        public int GoalAlignmentBonus { get; init; }

        [JsonPropertyName("efficiencyBonus")]
        public int EfficiencyBonus { get; init; }

        [JsonPropertyName("totalProjectedIncrease")]
        public int TotalProjectedIncrease { get; init; }

        [JsonPropertyName("newProjectedTotal")]
        public int NewProjectedTotal { get; init; }
    }

    public record ProgressPrediction
    {
        [JsonPropertyName("completionProbability")]
        public double CompletionProbability { get; init; } // 0.0 to 1.0

        [JsonPropertyName("estimatedCompletionDate")]
        public DateTime EstimatedCompletionDate { get; init; }

        [JsonPropertyName("remainingEffort")]
        public double RemainingEffortHours { get; init; }

        [JsonPropertyName("recommendedPace")]
        public string RecommendedPace { get; init; } = string.Empty; // "Slow down", "Maintain", "Speed up"

        [JsonPropertyName("riskFactors")]
        public List<string> RiskFactors { get; init; } = new();

        [JsonPropertyName("successFactors")]
        public List<string> SuccessFactors { get; init; } = new();
    }

    public record TaskGoalLink
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = Guid.NewGuid().ToString();

        [JsonPropertyName("taskId")]
        public string TaskId { get; init; } = string.Empty;

        [JsonPropertyName("goalId")]
        public string GoalId { get; init; } = string.Empty;

        [JsonPropertyName("milestoneId")]
        public string? MilestoneId { get; init; }

        [JsonPropertyName("contributionWeight")]
        public double ContributionWeight { get; init; }

        [JsonPropertyName("expectedProgressImpact")]
        public double? ExpectedProgressImpact { get; init; }

        [JsonPropertyName("actualProgressImpact")]
        public double? ActualProgressImpact { get; init; }

        [JsonPropertyName("zeitCoinBonusMultiplier")]
        public double ZeitCoinBonusMultiplier { get; init; }

        [JsonPropertyName("notes")]
        public string? Notes { get; init; }

        [JsonPropertyName("linkedAt")]
        public DateTime LinkedAt { get; init; } = DateTime.UtcNow;

        [JsonPropertyName("isActive")]
        public bool IsActive { get; init; } = true;
    }

    // 4. Validator
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.TaskId)
                .NotEmpty()
                .WithMessage("Task ID is required");

            RuleFor(x => x.GoalId)
                .NotEmpty()
                .WithMessage("Goal ID is required");

            RuleFor(x => x.ContributionWeight)
                .GreaterThan(0)
                .WithMessage("Contribution weight must be greater than 0")
                .LessThanOrEqualTo(1.0)
                .WithMessage("Contribution weight cannot exceed 1.0");

            RuleFor(x => x.ExpectedProgressImpact)
                .GreaterThan(0)
                .WithMessage("Expected progress impact must be positive")
                .LessThanOrEqualTo(1.0)
                .WithMessage("Expected progress impact cannot exceed 1.0 (100%)")
                .When(x => x.ExpectedProgressImpact.HasValue);

            RuleFor(x => x.ZeitCoinBonusMultiplier)
                .GreaterThanOrEqualTo(1.0)
                .WithMessage("ZeitCoin bonus multiplier must be at least 1.0")
                .LessThanOrEqualTo(3.0)
                .WithMessage("ZeitCoin bonus multiplier cannot exceed 3.0");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters");
        }
    }

    // 5. Handler
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICosmosRepository<TaskDocument> _taskRepository;
        private readonly ICosmosRepository<GoalDocument> _goalRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ICosmosRepository<TaskDocument> taskRepository,
            ICosmosRepository<GoalDocument> goalRepository,
            ILogger<Handler> logger)
        {
            _taskRepository = taskRepository;
            _goalRepository = goalRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Linking task {TaskId} to goal {GoalId} for user {UserId}", 
                    request.TaskId, request.GoalId, request.UserId);

                // 1. Fetch and validate task and goal
                var task = await _taskRepository.GetAsync(request.TaskId, request.UserId);
                if (task == null)
                {
                    return Result.FailureResult("Task not found");
                }

                if (task.UserId != request.UserId)
                {
                    return Result.FailureResult("Task does not belong to user");
                }

                var goal = await _goalRepository.GetAsync(request.GoalId, request.UserId);
                if (goal == null)
                {
                    return Result.FailureResult("Goal not found");
                }

                if (goal.UserId != request.UserId)
                {
                    return Result.FailureResult("Goal does not belong to user");
                }

                // 2. Check if already linked
                if (goal.LinkedTaskIds.Contains(request.TaskId))
                {
                    return Result.FailureResult("Task is already linked to this goal");
                }

                // 3. Validate milestone if specified
                if (!string.IsNullOrEmpty(request.MilestoneId))
                {
                    var milestone = goal.Milestones.FirstOrDefault(m => m.Id == request.MilestoneId);
                    if (milestone == null)
                    {
                        return Result.FailureResult("Specified milestone not found in goal");
                    }
                }

                // 4. Create task-goal link
                var link = new TaskGoalLink
                {
                    TaskId = request.TaskId,
                    GoalId = request.GoalId,
                    MilestoneId = request.MilestoneId,
                    ContributionWeight = request.ContributionWeight,
                    ExpectedProgressImpact = request.ExpectedProgressImpact,
                    ZeitCoinBonusMultiplier = request.ZeitCoinBonusMultiplier,
                    Notes = request.Notes
                };

                // 5. Update goal with linked task
                goal.LinkedTaskIds.Add(request.TaskId);
                
                // Store link in goal metadata
                if (!goal.Metadata.ContainsKey("taskLinks"))
                {
                    goal.Metadata["taskLinks"] = new List<TaskGoalLink>();
                }
                var taskLinks = (List<TaskGoalLink>)goal.Metadata["taskLinks"];
                taskLinks.Add(link);

                // 6. Update task with goal reference
                if (!task.Metadata.ContainsKey("linkedGoals"))
                {
                    task.Metadata["linkedGoals"] = new List<string>();
                }
                var linkedGoals = (List<string>)task.Metadata["linkedGoals"];
                if (!linkedGoals.Contains(request.GoalId))
                {
                    linkedGoals.Add(request.GoalId);
                }

                // 7. Calculate ZeitCoin impact
                var zeitCoinImpact = CalculateZeitCoinImpact(task, goal, link);
                
                // Update task ZeitCoin eligibility and multiplier
                if (goal.IsZeitCoinEligible && request.ZeitCoinBonusMultiplier > 1.0)
                {
                    task.IsZeitCoinEligible = true;
                    var currentMultiplier = task.Metadata.ContainsKey("zeitCoinMultiplier") 
                        ? Convert.ToDouble(task.Metadata["zeitCoinMultiplier"]) 
                        : 1.0;
                    task.Metadata["zeitCoinMultiplier"] = currentMultiplier * request.ZeitCoinBonusMultiplier;
                }

                // 8. Calculate progress if auto-calculate is enabled
                var progressInfo = CalculateGoalProgress(goal, request.AutoCalculateProgress);

                // 9. Generate progress prediction
                var prediction = GenerateProgressPrediction(goal, task);

                // 10. Generate recommended actions
                var recommendations = GenerateRecommendations(goal, task, link, progressInfo);

                // 11. Save updates
                goal.UpdatedAt = DateTime.UtcNow;
                goal.Version++;
                await _goalRepository.CreateOrUpdateAsync(goal, request.UserId);

                task.UpdatedAt = DateTime.UtcNow;
                await _taskRepository.CreateOrUpdateAsync(task, request.UserId);

                _logger.LogInformation("Successfully linked task {TaskId} to goal {GoalId}", 
                    request.TaskId, request.GoalId);

                return Result.SuccessResult(link.Id, progressInfo, zeitCoinImpact, prediction, recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking task {TaskId} to goal {GoalId}", 
                    request.TaskId, request.GoalId);
                return Result.FailureResult($"Failed to link task to goal: {ex.Message}");
            }
        }

        private ZeitCoinImpact CalculateZeitCoinImpact(TaskDocument task, GoalDocument goal, TaskGoalLink link)
        {
            if (!goal.IsZeitCoinEligible || !task.IsZeitCoinEligible)
            {
                return new ZeitCoinImpact();
            }

            // Base task ZeitCoin potential
            var basePoints = task.Metadata.ContainsKey("zeitCoinPoints") 
                ? Convert.ToInt32(task.Metadata["zeitCoinPoints"])
                : EstimateTaskZeitCoinPoints(task);

            // Goal alignment bonus (10-30% based on goal priority and category)
            var alignmentBonus = goal.Priority switch
            {
                "Critical" => (int)(basePoints * 0.3),
                "High" => (int)(basePoints * 0.2),
                "Medium" => (int)(basePoints * 0.15),
                "Low" => (int)(basePoints * 0.1),
                _ => (int)(basePoints * 0.15)
            };

            // Efficiency bonus for good planning (linking task to goal shows planning)
            var efficiencyBonus = (int)(basePoints * 0.1);

            // Apply link-specific multiplier
            var bonusPoints = (int)(basePoints * (link.ZeitCoinBonusMultiplier - 1.0));

            var totalIncrease = alignmentBonus + efficiencyBonus + bonusPoints;
            var newTotal = basePoints + totalIncrease;

            return new ZeitCoinImpact
            {
                BonusMultiplier = link.ZeitCoinBonusMultiplier,
                ProjectedBonusPoints = bonusPoints,
                GoalAlignmentBonus = alignmentBonus,
                EfficiencyBonus = efficiencyBonus,
                TotalProjectedIncrease = totalIncrease,
                NewProjectedTotal = newTotal
            };
        }

        private int EstimateTaskZeitCoinPoints(TaskDocument task)
        {
            // Estimate based on task properties
            var basePoints = 10; // Minimum

            // Add points based on estimated duration
            if (task.Metadata.ContainsKey("estimatedMinutes"))
            {
                var minutes = Convert.ToInt32(task.Metadata["estimatedMinutes"]);
                basePoints += Math.Min(minutes / 15, 100); // 1 point per 15 minutes, max 100
            }

            // Add points based on priority
            var priority = task.Metadata.ContainsKey("priority") 
                ? task.Metadata["priority"].ToString() 
                : "Medium";
            
            basePoints += priority switch
            {
                "Critical" => 50,
                "High" => 30,
                "Medium" => 20,
                "Low" => 10,
                _ => 20
            };

            return basePoints;
        }

        private GoalProgressInfo CalculateGoalProgress(GoalDocument goal, bool autoCalculate)
        {
            var previousProgress = goal.Progress;
            var currentProgress = previousProgress;

            if (autoCalculate)
            {
                // Calculate progress based on completed linked tasks
                var totalTasks = goal.LinkedTaskIds.Count;
                var completedTasks = 0;

                if (goal.Metadata.ContainsKey("taskLinks"))
                {
                    var taskLinks = (List<TaskGoalLink>)goal.Metadata["taskLinks"];
                    
                    // This would require fetching task statuses - simplified for now
                    // In real implementation, we'd check task completion status
                    completedTasks = taskLinks.Count(link => link.ActualProgressImpact.HasValue);
                }

                // Basic progress calculation (can be enhanced with weighted contributions)
                currentProgress = totalTasks > 0 ? (double)completedTasks / totalTasks : 0.0;
                goal.Progress = currentProgress;
            }

            // Calculate timeline predictions
            var estimatedCompletion = EstimateCompletionDate(goal);
            var isOnTrack = IsGoalOnTrack(goal, estimatedCompletion);
            var daysAheadBehind = CalculateDaysAheadBehind(goal, estimatedCompletion);

            return new GoalProgressInfo
            {
                GoalId = goal.Id,
                CurrentProgress = currentProgress,
                PreviousProgress = previousProgress,
                ProgressIncrease = currentProgress - previousProgress,
                TotalLinkedTasks = goal.LinkedTaskIds.Count,
                CompletedLinkedTasks = 0, // Would need to fetch actual task statuses
                EstimatedCompletionDate = estimatedCompletion,
                IsOnTrack = isOnTrack,
                DaysAheadBehind = daysAheadBehind
            };
        }

        private DateTime? EstimateCompletionDate(GoalDocument goal)
        {
            if (!goal.TargetDate.HasValue)
                return null;

            var remainingProgress = 1.0 - goal.Progress;
            if (remainingProgress <= 0)
                return DateTime.UtcNow; // Already complete

            var progressPerDay = goal.Progress / Math.Max(1, (DateTime.UtcNow - goal.CreatedAt).TotalDays);
            if (progressPerDay <= 0)
                return goal.TargetDate; // No progress yet, assume target date

            var daysToComplete = remainingProgress / progressPerDay;
            return DateTime.UtcNow.AddDays(daysToComplete);
        }

        private bool IsGoalOnTrack(GoalDocument goal, DateTime? estimatedCompletion)
        {
            if (!goal.TargetDate.HasValue || !estimatedCompletion.HasValue)
                return true; // No deadline to compare

            return estimatedCompletion.Value <= goal.TargetDate.Value;
        }

        private int CalculateDaysAheadBehind(GoalDocument goal, DateTime? estimatedCompletion)
        {
            if (!goal.TargetDate.HasValue || !estimatedCompletion.HasValue)
                return 0;

            return (int)(goal.TargetDate.Value - estimatedCompletion.Value).TotalDays;
        }

        private ProgressPrediction GenerateProgressPrediction(GoalDocument goal, TaskDocument task)
        {
            var completionProbability = CalculateCompletionProbability(goal);
            var estimatedCompletion = EstimateCompletionDate(goal) ?? goal.TargetDate ?? DateTime.UtcNow.AddDays(30);
            var remainingEffort = CalculateRemainingEffort(goal);
            var recommendedPace = DetermineRecommendedPace(goal);
            var riskFactors = IdentifyRiskFactors(goal);
            var successFactors = IdentifySuccessFactors(goal);

            return new ProgressPrediction
            {
                CompletionProbability = completionProbability,
                EstimatedCompletionDate = estimatedCompletion,
                RemainingEffortHours = remainingEffort,
                RecommendedPace = recommendedPace,
                RiskFactors = riskFactors,
                SuccessFactors = successFactors
            };
        }

        private double CalculateCompletionProbability(GoalDocument goal)
        {
            var probability = 0.7; // Base optimistic probability

            // Adjust based on progress rate
            var progressRate = goal.Progress / Math.Max(1, (DateTime.UtcNow - goal.CreatedAt).TotalDays);
            if (progressRate > 0.02) probability += 0.1; // Good daily progress
            if (progressRate < 0.005) probability -= 0.2; // Slow progress

            // Adjust based on linked tasks
            if (goal.LinkedTaskIds.Count > 0) probability += 0.1; // Has concrete steps
            if (goal.LinkedTaskIds.Count > 10) probability -= 0.1; // Maybe too complex

            // Adjust based on timeline
            if (goal.TargetDate.HasValue)
            {
                var daysRemaining = (goal.TargetDate.Value - DateTime.UtcNow).TotalDays;
                if (daysRemaining < 7) probability -= 0.2; // Very tight deadline
                if (daysRemaining > 365) probability -= 0.1; // Very long term
            }

            return Math.Max(0.1, Math.Min(0.95, probability));
        }

        private double CalculateRemainingEffort(GoalDocument goal)
        {
            var estimatedTotal = goal.EstimatedHours ?? 40; // Default estimate
            var actualSpent = goal.ActualHours ?? 0;
            var remainingByProgress = estimatedTotal * (1.0 - goal.Progress);
            var remainingByTime = Math.Max(0, estimatedTotal - actualSpent);

            return Math.Max(remainingByProgress, remainingByTime);
        }

        private string DetermineRecommendedPace(GoalDocument goal)
        {
            if (!goal.TargetDate.HasValue)
                return "Maintain";

            var daysRemaining = (goal.TargetDate.Value - DateTime.UtcNow).TotalDays;
            var progressRemaining = 1.0 - goal.Progress;
            var requiredDailyProgress = progressRemaining / Math.Max(1, daysRemaining);
            var currentDailyProgress = goal.Progress / Math.Max(1, (DateTime.UtcNow - goal.CreatedAt).TotalDays);

            if (requiredDailyProgress > currentDailyProgress * 1.5)
                return "Speed up";
            else if (requiredDailyProgress < currentDailyProgress * 0.8)
                return "Slow down";
            else
                return "Maintain";
        }

        private List<string> IdentifyRiskFactors(GoalDocument goal)
        {
            var risks = new List<string>();

            if (goal.LinkedTaskIds.Count == 0)
                risks.Add("No concrete tasks defined yet");

            if (goal.Progress == 0 && (DateTime.UtcNow - goal.CreatedAt).TotalDays > 7)
                risks.Add("No progress made in over a week");

            if (goal.TargetDate.HasValue && (goal.TargetDate.Value - DateTime.UtcNow).TotalDays < 14)
                risks.Add("Very tight deadline approaching");

            if (goal.EstimatedHours > 200)
                risks.Add("Large time investment may be overwhelming");

            return risks;
        }

        private List<string> IdentifySuccessFactors(GoalDocument goal)
        {
            var factors = new List<string>();

            if (goal.LinkedTaskIds.Count > 0)
                factors.Add("Has actionable tasks defined");

            if (goal.Progress > 0)
                factors.Add("Already making progress");

            if (goal.Milestones.Count > 0)
                factors.Add("Broken down into milestones");

            if (goal.IsZeitCoinEligible)
                factors.Add("ZeitCoin rewards provide motivation");

            if (!string.IsNullOrEmpty(goal.Description))
                factors.Add("Clear description and purpose");

            return factors;
        }

        private List<string> GenerateRecommendations(GoalDocument goal, TaskDocument task, 
            TaskGoalLink link, GoalProgressInfo progress)
        {
            var recommendations = new List<string>();

            if (progress.CurrentProgress == 0)
            {
                recommendations.Add("Start with the linked task to build momentum");
            }

            if (goal.LinkedTaskIds.Count < 3)
            {
                recommendations.Add("Consider breaking down the goal into more specific tasks");
            }

            if (link.ZeitCoinBonusMultiplier == 1.0 && goal.IsZeitCoinEligible)
            {
                recommendations.Add("Set a ZeitCoin bonus multiplier to increase motivation");
            }

            if (goal.Milestones.Count == 0 && goal.EstimatedHours > 20)
            {
                recommendations.Add("Create milestones to track intermediate progress");
            }

            if (!progress.IsOnTrack && goal.TargetDate.HasValue)
            {
                recommendations.Add("Consider adjusting timeline or breaking into smaller goals");
            }

            return recommendations;
        }
    }

    // 6. API Endpoint
    [ApiController]
    [Route("api/goals")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Link a task to a goal with automatic progress calculation and ZeitCoin optimization
        /// </summary>
        /// <param name="command">Task-goal linking details</param>
        /// <returns>Updated goal progress and ZeitCoin projections</returns>
        [HttpPost("link-task")]
        public async Task<ActionResult<Result>> LinkTaskToGoal([FromBody] Command command)
        {
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Quick link task to goal with smart defaults
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="taskId">Task to link</param>
        /// <param name="goalId">Goal to link to</param>
        /// <param name="contributionWeight">How much this task contributes (0.1-1.0)</param>
        /// <param name="zeitCoinBonus">ZeitCoin bonus multiplier (1.0-3.0)</param>
        /// <returns>Link result with progress and recommendations</returns>
        [HttpPost("quick-link")]
        public async Task<ActionResult<Result>> QuickLinkTaskToGoal(
            [FromQuery] string userId,
            [FromQuery] string taskId,
            [FromQuery] string goalId,
            [FromQuery] double contributionWeight = 1.0,
            [FromQuery] double zeitCoinBonus = 1.2)
        {
            var command = new Command
            {
                UserId = userId,
                TaskId = taskId,
                GoalId = goalId,
                ContributionWeight = contributionWeight,
                ZeitCoinBonusMultiplier = zeitCoinBonus,
                AutoCalculateProgress = true
            };

            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}