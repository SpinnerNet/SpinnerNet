using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Goals;

/// <summary>
/// Vertical slice for creating SMART goals with ZeitCoin integration
/// Implements: Command → Validation → Handler → Endpoint pattern
/// Goals drive task creation and ZeitCoin earning strategies
/// </summary>
public static class CreateGoal
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("category")]
        public string Category { get; init; } = "Personal";

        [JsonPropertyName("priority")]
        public GoalPriority Priority { get; init; } = GoalPriority.Medium;

        [JsonPropertyName("targetValue")]
        public double? TargetValue { get; init; }

        [JsonPropertyName("targetUnit")]
        public string? TargetUnit { get; init; }

        [JsonPropertyName("targetDate")]
        public DateTime? TargetDate { get; init; }

        [JsonPropertyName("estimatedHours")]
        public int? EstimatedHours { get; init; }

        [JsonPropertyName("isZeitCoinEligible")]
        public bool IsZeitCoinEligible { get; init; } = true;

        [JsonPropertyName("zeitCoinMultiplier")]
        public double ZeitCoinMultiplier { get; init; } = 1.0;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; init; } = new();

        [JsonPropertyName("milestones")]
        public List<GoalMilestone> Milestones { get; init; } = new();

        [JsonPropertyName("aiGeneratedInsights")]
        public bool AiGeneratedInsights { get; init; } = true;
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("goalId")]
        public string? GoalId { get; init; }

        [JsonPropertyName("goal")]
        public GoalDocument? Goal { get; init; }

        [JsonPropertyName("aiInsights")]
        public GoalAiInsights? AiInsights { get; init; }

        [JsonPropertyName("recommendedTasks")]
        public List<TaskRecommendation> RecommendedTasks { get; init; } = new();

        [JsonPropertyName("zeitCoinProjection")]
        public ZeitCoinProjection? ZeitCoinProjection { get; init; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(GoalDocument goal, GoalAiInsights? insights = null, 
            List<TaskRecommendation>? tasks = null, ZeitCoinProjection? projection = null) =>
            new()
            {
                Success = true,
                GoalId = goal.Id,
                Goal = goal,
                AiInsights = insights,
                RecommendedTasks = tasks ?? new(),
                ZeitCoinProjection = projection
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
    public enum GoalPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public record GoalMilestone
    {
        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("targetDate")]
        public DateTime? TargetDate { get; init; }

        [JsonPropertyName("targetValue")]
        public double? TargetValue { get; init; }

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; init; }

        [JsonPropertyName("completedAt")]
        public DateTime? CompletedAt { get; init; }
    }

    public record GoalAiInsights
    {
        [JsonPropertyName("smartnessScore")]
        public int SmartnessScore { get; init; } // 1-10 how SMART the goal is

        [JsonPropertyName("feasibilityScore")]
        public int FeasibilityScore { get; init; } // 1-10 how achievable

        [JsonPropertyName("suggestions")]
        public List<string> Suggestions { get; init; } = new();

        [JsonPropertyName("breakdownStrategy")]
        public string? BreakdownStrategy { get; init; }

        [JsonPropertyName("riskFactors")]
        public List<string> RiskFactors { get; init; } = new();

        [JsonPropertyName("optimizationTips")]
        public List<string> OptimizationTips { get; init; } = new();
    }

    public record TaskRecommendation
    {
        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("estimatedHours")]
        public double EstimatedHours { get; init; }

        [JsonPropertyName("priority")]
        public string Priority { get; init; } = "Medium";

        [JsonPropertyName("category")]
        public string Category { get; init; } = string.Empty;

        [JsonPropertyName("zeitCoinPotential")]
        public int ZeitCoinPotential { get; init; }

        [JsonPropertyName("aiRationale")]
        public string? AiRationale { get; init; }
    }

    public record ZeitCoinProjection
    {
        [JsonPropertyName("estimatedPoints")]
        public int EstimatedPoints { get; init; }

        [JsonPropertyName("timeToCompletion")]
        public int TimeToCompletionDays { get; init; }

        [JsonPropertyName("monthlyEarningPotential")]
        public int MonthlyEarningPotential { get; init; }

        [JsonPropertyName("efficiencyBonus")]
        public double EfficiencyBonus { get; init; }

        [JsonPropertyName("categoryMultiplier")]
        public double CategoryMultiplier { get; init; }
    }

    // 4. Validator
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Goal title is required")
                .MinimumLength(3)
                .WithMessage("Goal title must be at least 3 characters")
                .MaximumLength(200)
                .WithMessage("Goal title cannot exceed 200 characters");

            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage("Goal category is required")
                .Must(BeValidCategory)
                .WithMessage("Invalid goal category");

            RuleFor(x => x.Priority)
                .IsInEnum()
                .WithMessage("Invalid priority level");

            RuleFor(x => x.TargetDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Target date must be in the future")
                .When(x => x.TargetDate.HasValue);

            RuleFor(x => x.EstimatedHours)
                .GreaterThan(0)
                .WithMessage("Estimated hours must be positive")
                .LessThanOrEqualTo(10000)
                .WithMessage("Estimated hours seems unrealistic (max 10,000)")
                .When(x => x.EstimatedHours.HasValue);

            RuleFor(x => x.ZeitCoinMultiplier)
                .GreaterThan(0)
                .WithMessage("ZeitCoin multiplier must be positive")
                .LessThanOrEqualTo(5.0)
                .WithMessage("ZeitCoin multiplier cannot exceed 5.0");

            RuleFor(x => x.TargetValue)
                .GreaterThan(0)
                .WithMessage("Target value must be positive")
                .When(x => x.TargetValue.HasValue);

            RuleForEach(x => x.Tags)
                .NotEmpty()
                .WithMessage("Tags cannot be empty")
                .MaximumLength(50)
                .WithMessage("Tag length cannot exceed 50 characters");

            RuleFor(x => x.Tags)
                .Must(tags => tags.Count <= 10)
                .WithMessage("Cannot have more than 10 tags");

            RuleFor(x => x.Milestones)
                .Must(milestones => milestones.Count <= 20)
                .WithMessage("Cannot have more than 20 milestones");

            RuleForEach(x => x.Milestones)
                .SetValidator(new MilestoneValidator());
        }

        private static bool BeValidCategory(string category)
        {
            var validCategories = new[]
            {
                "Personal", "Professional", "Health", "Finance", "Learning", 
                "Relationships", "Creativity", "Business", "Fitness", "Productivity"
            };
            return validCategories.Contains(category);
        }
    }

    public class MilestoneValidator : AbstractValidator<GoalMilestone>
    {
        public MilestoneValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Milestone title is required")
                .MaximumLength(100)
                .WithMessage("Milestone title cannot exceed 100 characters");

            RuleFor(x => x.TargetDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Milestone target date must be in the future")
                .When(x => x.TargetDate.HasValue);
        }
    }

    // 5. Handler
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICosmosRepository<GoalDocument> _goalRepository;
        private readonly ICosmosRepository<UserDocument> _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ICosmosRepository<GoalDocument> goalRepository,
            ICosmosRepository<UserDocument> userRepository,
            ILogger<Handler> logger)
        {
            _goalRepository = goalRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating goal for user {UserId}: {Title}", request.UserId, request.Title);

                // 1. Verify user exists
                var user = await _userRepository.GetAsync(request.UserId, request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for goal creation", request.UserId);
                    return Result.FailureResult("User not found");
                }

                // 2. Create goal document
                var goal = new GoalDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = request.UserId,
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim(),
                    Category = request.Category,
                    Priority = request.Priority.ToString(),
                    TargetValue = request.TargetValue,
                    TargetUnit = request.TargetUnit?.Trim(),
                    TargetDate = request.TargetDate,
                    EstimatedHours = request.EstimatedHours,
                    IsZeitCoinEligible = request.IsZeitCoinEligible,
                    ZeitCoinMultiplier = request.ZeitCoinMultiplier,
                    Tags = request.Tags.Select(t => t.Trim()).ToList(),
                    Status = "Active",
                    Progress = 0.0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "aiGenerated", request.AiGeneratedInsights },
                        { "smartnessVersion", "1.0" },
                        { "zeitCoinEligible", request.IsZeitCoinEligible }
                    }
                };

                // 3. Add milestones to metadata
                if (request.Milestones.Any())
                {
                    goal.Metadata["milestones"] = request.Milestones;
                }

                // 4. Generate AI insights if requested
                GoalAiInsights? aiInsights = null;
                List<TaskRecommendation> recommendedTasks = new();
                ZeitCoinProjection? zeitCoinProjection = null;

                if (request.AiGeneratedInsights)
                {
                    aiInsights = GenerateAiInsights(goal);
                    recommendedTasks = GenerateTaskRecommendations(goal, aiInsights);
                    
                    if (request.IsZeitCoinEligible)
                    {
                        zeitCoinProjection = CalculateZeitCoinProjection(goal, recommendedTasks);
                    }
                }

                // 5. Save goal to repository
                var savedGoal = await _goalRepository.CreateOrUpdateAsync(goal, request.UserId);

                _logger.LogInformation("Successfully created goal {GoalId} for user {UserId}", savedGoal.Id, request.UserId);

                return Result.SuccessResult(savedGoal, aiInsights, recommendedTasks, zeitCoinProjection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating goal for user {UserId}", request.UserId);
                return Result.FailureResult($"Failed to create goal: {ex.Message}");
            }
        }

        private GoalAiInsights GenerateAiInsights(GoalDocument goal)
        {
            // AI-powered SMART goal analysis
            var smartnessScore = CalculateSmartnessScore(goal);
            var feasibilityScore = CalculateFeasibilityScore(goal);

            var suggestions = new List<string>();
            var riskFactors = new List<string>();
            var optimizationTips = new List<string>();

            // SMART criteria analysis
            if (string.IsNullOrEmpty(goal.Description))
            {
                suggestions.Add("Add a detailed description to make the goal more specific");
                smartnessScore -= 1;
            }

            if (!goal.TargetValue.HasValue && string.IsNullOrEmpty(goal.TargetUnit))
            {
                suggestions.Add("Define measurable metrics (target value and unit)");
                smartnessScore -= 2;
            }

            if (!goal.TargetDate.HasValue)
            {
                suggestions.Add("Set a specific target date to create urgency");
                smartnessScore -= 1;
            }
            else if (goal.TargetDate < DateTime.UtcNow.AddDays(7))
            {
                riskFactors.Add("Very short timeline may create unnecessary pressure");
            }

            if (!goal.EstimatedHours.HasValue)
            {
                suggestions.Add("Estimate time investment for better planning");
            }
            else if (goal.EstimatedHours > 1000)
            {
                riskFactors.Add("Large time investment - consider breaking into smaller goals");
                feasibilityScore -= 2;
            }

            // Category-specific insights
            switch (goal.Category.ToLower())
            {
                case "health":
                case "fitness":
                    optimizationTips.Add("Track daily habits rather than just end results");
                    optimizationTips.Add("Consider accountability partners or apps");
                    break;
                case "learning":
                case "professional":
                    optimizationTips.Add("Schedule regular learning sessions");
                    optimizationTips.Add("Create practical application opportunities");
                    break;
                case "finance":
                    optimizationTips.Add("Break down into monthly savings targets");
                    optimizationTips.Add("Automate where possible to reduce friction");
                    break;
            }

            // ZeitCoin optimization
            if (goal.IsZeitCoinEligible)
            {
                optimizationTips.Add("Focus on efficiency and value creation for maximum ZeitCoin rewards");
                optimizationTips.Add("Document your process to help others and earn bonus points");
            }

            return new GoalAiInsights
            {
                SmartnessScore = Math.Max(1, Math.Min(10, smartnessScore)),
                FeasibilityScore = Math.Max(1, Math.Min(10, feasibilityScore)),
                Suggestions = suggestions,
                BreakdownStrategy = GenerateBreakdownStrategy(goal),
                RiskFactors = riskFactors,
                OptimizationTips = optimizationTips
            };
        }

        private int CalculateSmartnessScore(GoalDocument goal)
        {
            int score = 5; // Base score

            // Specific
            if (!string.IsNullOrEmpty(goal.Description) && goal.Description.Length > 20) score += 1;
            
            // Measurable
            if (goal.TargetValue.HasValue || !string.IsNullOrEmpty(goal.TargetUnit)) score += 2;
            
            // Achievable (based on estimated hours)
            if (goal.EstimatedHours.HasValue && goal.EstimatedHours <= 500) score += 1;
            
            // Relevant (has category and tags)
            if (!string.IsNullOrEmpty(goal.Category) && goal.Tags.Any()) score += 1;
            
            // Time-bound
            if (goal.TargetDate.HasValue) score += 1;

            return score;
        }

        private int CalculateFeasibilityScore(GoalDocument goal)
        {
            int score = 7; // Optimistic base

            if (goal.TargetDate.HasValue)
            {
                var timeToGoal = (goal.TargetDate.Value - DateTime.UtcNow).TotalDays;
                if (timeToGoal < 7) score -= 2; // Very tight deadline
                else if (timeToGoal > 365) score -= 1; // Very long term
            }

            if (goal.EstimatedHours.HasValue)
            {
                if (goal.EstimatedHours > 2000) score -= 3; // Massive undertaking
                else if (goal.EstimatedHours > 500) score -= 1; // Large project
            }

            return Math.Max(1, score);
        }

        private string GenerateBreakdownStrategy(GoalDocument goal)
        {
            var strategies = new List<string>();

            if (goal.EstimatedHours.HasValue && goal.EstimatedHours > 100)
            {
                strategies.Add("Break into monthly phases");
                strategies.Add("Set weekly milestones");
            }

            if (goal.TargetDate.HasValue)
            {
                var daysToGoal = (goal.TargetDate.Value - DateTime.UtcNow).TotalDays;
                if (daysToGoal > 90)
                {
                    strategies.Add("Create quarterly checkpoints");
                }
                strategies.Add("Schedule weekly progress reviews");
            }

            strategies.Add("Start with the easiest actionable steps");
            strategies.Add("Build momentum with quick wins");

            return string.Join(". ", strategies) + ".";
        }

        private List<TaskRecommendation> GenerateTaskRecommendations(GoalDocument goal, GoalAiInsights insights)
        {
            var recommendations = new List<TaskRecommendation>();

            // Generate 3-5 starter tasks based on goal
            switch (goal.Category.ToLower())
            {
                case "learning":
                    recommendations.AddRange(new[]
                    {
                        new TaskRecommendation
                        {
                            Title = $"Research and plan {goal.Title} learning path",
                            Description = "Identify key resources, courses, and materials needed",
                            EstimatedHours = 2,
                            Priority = "High",
                            Category = goal.Category,
                            ZeitCoinPotential = 50,
                            AiRationale = "Planning phase is crucial for learning efficiency"
                        },
                        new TaskRecommendation
                        {
                            Title = "Set up learning environment and tools",
                            Description = "Prepare workspace, install software, gather materials",
                            EstimatedHours = 1,
                            Priority = "Medium",
                            Category = goal.Category,
                            ZeitCoinPotential = 30,
                            AiRationale = "Proper setup reduces friction and increases consistency"
                        }
                    });
                    break;

                case "health":
                case "fitness":
                    recommendations.AddRange(new[]
                    {
                        new TaskRecommendation
                        {
                            Title = "Create baseline measurements and tracking system",
                            Description = "Document current state and set up progress tracking",
                            EstimatedHours = 1,
                            Priority = "High",
                            Category = goal.Category,
                            ZeitCoinPotential = 40,
                            AiRationale = "Measurement is essential for health goal success"
                        },
                        new TaskRecommendation
                        {
                            Title = "Design weekly action plan",
                            Description = "Plan specific activities, schedule, and meal prep",
                            EstimatedHours = 2,
                            Priority = "High", 
                            Category = goal.Category,
                            ZeitCoinPotential = 60,
                            AiRationale = "Structured planning dramatically improves adherence"
                        }
                    });
                    break;

                default:
                    recommendations.Add(new TaskRecommendation
                    {
                        Title = $"Break down {goal.Title} into actionable steps",
                        Description = "Create detailed project plan with specific tasks",
                        EstimatedHours = 1.5,
                        Priority = "High",
                        Category = goal.Category,
                        ZeitCoinPotential = 45,
                        AiRationale = "Clear planning reduces overwhelm and increases success rate"
                    });
                    break;
            }

            // Add common foundational task
            recommendations.Add(new TaskRecommendation
            {
                Title = "Set up progress tracking and accountability",
                Description = "Choose tracking method and accountability system",
                EstimatedHours = 0.5,
                Priority = "Medium",
                Category = goal.Category,
                ZeitCoinPotential = 25,
                AiRationale = "Accountability significantly increases goal completion rates"
            });

            return recommendations;
        }

        private ZeitCoinProjection CalculateZeitCoinProjection(GoalDocument goal, List<TaskRecommendation> tasks)
        {
            var basePoints = tasks.Sum(t => t.ZeitCoinPotential);
            
            // Apply goal-level multipliers
            var categoryMultiplier = goal.Category.ToLower() switch
            {
                "learning" => 1.3,
                "health" => 1.2,
                "professional" => 1.4,
                "productivity" => 1.5,
                _ => 1.0
            };

            var priorityMultiplier = goal.Priority switch
            {
                "Critical" => 1.5,
                "High" => 1.2,
                "Medium" => 1.0,
                "Low" => 0.8,
                _ => 1.0
            };

            var estimatedPoints = (int)(basePoints * categoryMultiplier * priorityMultiplier * goal.ZeitCoinMultiplier);
            
            var timeToCompletion = goal.TargetDate.HasValue 
                ? Math.Max(1, (int)(goal.TargetDate.Value - DateTime.UtcNow).TotalDays)
                : goal.EstimatedHours ?? 30;

            var monthlyPotential = timeToCompletion > 30 
                ? (int)(estimatedPoints * 30.0 / timeToCompletion)
                : estimatedPoints;

            return new ZeitCoinProjection
            {
                EstimatedPoints = estimatedPoints,
                TimeToCompletionDays = timeToCompletion,
                MonthlyEarningPotential = monthlyPotential,
                EfficiencyBonus = goal.ZeitCoinMultiplier - 1.0,
                CategoryMultiplier = categoryMultiplier
            };
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
        /// Create a new SMART goal with AI insights and ZeitCoin integration
        /// </summary>
        /// <param name="command">Goal creation details</param>
        /// <returns>Created goal with AI recommendations and ZeitCoin projections</returns>
        [HttpPost]
        public async Task<ActionResult<Result>> CreateGoal([FromBody] Command command)
        {
            var result = await _mediator.Send(command);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Create goal with AI assistance for better SMART goal formation
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="title">Goal title</param>
        /// <param name="category">Goal category</param>
        /// <param name="description">Optional description</param>
        /// <param name="estimatedHours">Time investment estimate</param>
        /// <param name="targetDate">Target completion date</param>
        /// <param name="zeitCoinEligible">Enable ZeitCoin rewards</param>
        /// <returns>Created goal with comprehensive AI analysis</returns>
        [HttpPost("create-with-ai")]
        public async Task<ActionResult<Result>> CreateGoalWithAi(
            [FromQuery] string userId,
            [FromQuery] string title,
            [FromQuery] string category = "Personal",
            [FromQuery] string? description = null,
            [FromQuery] int? estimatedHours = null,
            [FromQuery] DateTime? targetDate = null,
            [FromQuery] bool zeitCoinEligible = true)
        {
            var command = new Command
            {
                UserId = userId,
                Title = title,
                Description = description,
                Category = category,
                EstimatedHours = estimatedHours,
                TargetDate = targetDate,
                IsZeitCoinEligible = zeitCoinEligible,
                AiGeneratedInsights = true,
                Priority = GoalPriority.Medium
            };

            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}