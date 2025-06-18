using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Analytics;

/// <summary>
/// Vertical slice for retrieving user analytics and time banking foundation data
/// Core Zeitsparkasse feature for ZeitCoin economy foundation - tracks productivity patterns
/// </summary>
public static class GetUserAnalytics
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("timeRange")]
        public TimeRange TimeRange { get; init; } = TimeRange.Last30Days;

        [JsonPropertyName("includeZeitCoinMetrics")]
        public bool IncludeZeitCoinMetrics { get; init; } = true;

        [JsonPropertyName("includeProductivityInsights")]
        public bool IncludeProductivityInsights { get; init; } = true;

        [JsonPropertyName("includeTimePatterns")]
        public bool IncludeTimePatterns { get; init; } = true;

        [JsonPropertyName("categories")]
        public List<string>? Categories { get; init; }
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("analytics")]
        public UserAnalytics? Analytics { get; init; }

        [JsonPropertyName("zeitCoinMetrics")]
        public ZeitCoinMetrics? ZeitCoinMetrics { get; init; }

        [JsonPropertyName("productivityInsights")]
        public List<ProductivityInsight> ProductivityInsights { get; init; } = new();

        [JsonPropertyName("timePatterns")]
        public TimePatterns? TimePatterns { get; init; }

        [JsonPropertyName("recommendations")]
        public List<AnalyticsRecommendation> Recommendations { get; init; } = new();

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(UserAnalytics analytics, ZeitCoinMetrics? zeitCoin, 
            List<ProductivityInsight> insights, TimePatterns? patterns, List<AnalyticsRecommendation> recommendations) =>
            new() 
            { 
                Success = true, 
                Analytics = analytics,
                ZeitCoinMetrics = zeitCoin,
                ProductivityInsights = insights,
                TimePatterns = patterns,
                Recommendations = recommendations
            };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
            new() { Success = false, ValidationErrors = validationErrors };
    }

    // 3. Supporting Models
    public enum TimeRange
    {
        Last7Days = 0,
        Last30Days = 1,
        Last90Days = 2,
        ThisYear = 3,
        AllTime = 4
    }

    public record UserAnalytics
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("timeRange")]
        public string TimeRange { get; init; } = string.Empty;

        [JsonPropertyName("totalTasks")]
        public int TotalTasks { get; init; }

        [JsonPropertyName("completedTasks")]
        public int CompletedTasks { get; init; }

        [JsonPropertyName("completionRate")]
        public double CompletionRate { get; init; }

        [JsonPropertyName("totalTimeSpentHours")]
        public double TotalTimeSpentHours { get; init; }

        [JsonPropertyName("averageTaskDurationMinutes")]
        public double AverageTaskDurationMinutes { get; init; }

        [JsonPropertyName("averageProductivityRating")]
        public double AverageProductivityRating { get; init; }

        [JsonPropertyName("averageSatisfactionRating")]
        public double AverageSatisfactionRating { get; init; }

        [JsonPropertyName("timeEstimationAccuracy")]
        public double TimeEstimationAccuracy { get; init; }

        [JsonPropertyName("categoryBreakdown")]
        public Dictionary<string, CategoryStats> CategoryBreakdown { get; init; } = new();

        [JsonPropertyName("dailyActivityCount")]
        public Dictionary<string, int> DailyActivityCount { get; init; } = new();

        [JsonPropertyName("productivityTrends")]
        public List<ProductivityDataPoint> ProductivityTrends { get; init; } = new();

        [JsonPropertyName("generatedAt")]
        public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    }

    public record CategoryStats
    {
        [JsonPropertyName("taskCount")]
        public int TaskCount { get; init; }

        [JsonPropertyName("completedCount")]
        public int CompletedCount { get; init; }

        [JsonPropertyName("totalTimeHours")]
        public double TotalTimeHours { get; init; }

        [JsonPropertyName("averageProductivity")]
        public double AverageProductivity { get; init; }

        [JsonPropertyName("averageSatisfaction")]
        public double AverageSatisfaction { get; init; }

        [JsonPropertyName("completionRate")]
        public double CompletionRate { get; init; }
    }

    public record ProductivityDataPoint
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; init; }

        [JsonPropertyName("tasksCompleted")]
        public int TasksCompleted { get; init; }

        [JsonPropertyName("timeSpentHours")]
        public double TimeSpentHours { get; init; }

        [JsonPropertyName("averageProductivity")]
        public double AverageProductivity { get; init; }

        [JsonPropertyName("averageSatisfaction")]
        public double AverageSatisfaction { get; init; }

        [JsonPropertyName("zeitCoinPointsEarned")]
        public int ZeitCoinPointsEarned { get; init; }
    }

    public record ZeitCoinMetrics
    {
        [JsonPropertyName("totalPointsEarned")]
        public int TotalPointsEarned { get; init; }

        [JsonPropertyName("eligibleTasks")]
        public int EligibleTasks { get; init; }

        [JsonPropertyName("eligibilityRate")]
        public double EligibilityRate { get; init; }

        [JsonPropertyName("averagePointsPerTask")]
        public double AveragePointsPerTask { get; init; }

        [JsonPropertyName("projectedMonthlyEarnings")]
        public int ProjectedMonthlyEarnings { get; init; }

        [JsonPropertyName("timeToValueBaseline")]
        public double TimeToValueBaseline { get; init; }

        [JsonPropertyName("efficiencyBaseline")]
        public double EfficiencyBaseline { get; init; }

        [JsonPropertyName("valueCreationScore")]
        public double ValueCreationScore { get; init; }

        [JsonPropertyName("optimizationPotential")]
        public double OptimizationPotential { get; init; }

        [JsonPropertyName("categoryEarnings")]
        public Dictionary<string, int> CategoryEarnings { get; init; } = new();
    }

    public record TimePatterns
    {
        [JsonPropertyName("mostProductiveHours")]
        public List<int> MostProductiveHours { get; init; } = new();

        [JsonPropertyName("mostProductiveDays")]
        public List<string> MostProductiveDays { get; init; } = new();

        [JsonPropertyName("averageSessionLength")]
        public double AverageSessionLength { get; init; }

        [JsonPropertyName("peakEfficiencyWindows")]
        public List<TimeWindow> PeakEfficiencyWindows { get; init; } = new();

        [JsonPropertyName("workLifeBalance")]
        public WorkLifeBalanceMetrics WorkLifeBalance { get; init; } = new();

        [JsonPropertyName("seasonalTrends")]
        public Dictionary<string, double> SeasonalTrends { get; init; } = new();
    }

    public record TimeWindow
    {
        [JsonPropertyName("startHour")]
        public int StartHour { get; init; }

        [JsonPropertyName("endHour")]
        public int EndHour { get; init; }

        [JsonPropertyName("averageProductivity")]
        public double AverageProductivity { get; init; }

        [JsonPropertyName("taskCount")]
        public int TaskCount { get; init; }
    }

    public record WorkLifeBalanceMetrics
    {
        [JsonPropertyName("workHoursPercentage")]
        public double WorkHoursPercentage { get; init; }

        [JsonPropertyName("personalHoursPercentage")]
        public double PersonalHoursPercentage { get; init; }

        [JsonPropertyName("healthHoursPercentage")]
        public double HealthHoursPercentage { get; init; }

        [JsonPropertyName("balanceScore")]
        public double BalanceScore { get; set; }

        [JsonPropertyName("recommendations")]
        public List<string> Recommendations { get; set; } = new();
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
        public string Impact { get; init; } = "medium";

        [JsonPropertyName("trend")]
        public string Trend { get; init; } = "stable"; // improving, declining, stable

        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; init; } = new();

        [JsonPropertyName("actionable")]
        public bool Actionable { get; init; } = true;
    }

    public record AnalyticsRecommendation
    {
        [JsonPropertyName("category")]
        public string Category { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("priority")]
        public string Priority { get; init; } = "medium"; // high, medium, low

        [JsonPropertyName("potentialImpact")]
        public string PotentialImpact { get; init; } = string.Empty;

        [JsonPropertyName("timeToImplement")]
        public string TimeToImplement { get; init; } = string.Empty;

        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    // 4. Validator (Input Validation)
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MaximumLength(100).WithMessage("User ID must not exceed 100 characters");

            RuleFor(x => x.TimeRange)
                .IsInEnum().WithMessage("Time range must be a valid option");

            RuleFor(x => x.Categories)
                .Must(categories => categories == null || categories.All(c => !string.IsNullOrWhiteSpace(c)))
                .WithMessage("All categories must be non-empty if provided");
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
                _logger.LogInformation("Generating analytics for user {UserId} with time range {TimeRange}", 
                    request.UserId, request.TimeRange);

                // 1. Verify user exists
                var users = await _userRepository.QueryAsync(
                    u => u.UserId == request.UserId,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (!users.Any())
                {
                    _logger.LogWarning("Analytics requested for non-existent user: {UserId}", request.UserId);
                    return Result.Failure("User not found");
                }

                var user = users.First();

                // 2. Determine date range
                var (startDate, endDate) = GetDateRange(request.TimeRange);
                
                _logger.LogDebug("Analytics date range: {StartDate} to {EndDate}", startDate, endDate);

                // 3. Fetch user tasks in date range
                var tasks = await _taskRepository.QueryAsync(
                    t => t.UserId == request.UserId && 
                         t.CreatedAt >= startDate && 
                         t.CreatedAt <= endDate &&
                         (request.Categories == null || request.Categories.Contains(t.Category)),
                    cancellationToken: cancellationToken);

                _logger.LogDebug("Found {TaskCount} tasks for analytics", tasks.Count);

                // 4. Generate core analytics
                var analytics = GenerateUserAnalytics(request.UserId, tasks, request.TimeRange.ToString(), startDate, endDate);

                // 5. Generate ZeitCoin metrics if requested
                ZeitCoinMetrics? zeitCoinMetrics = null;
                if (request.IncludeZeitCoinMetrics)
                {
                    zeitCoinMetrics = GenerateZeitCoinMetrics(tasks);
                }

                // 6. Generate productivity insights if requested
                var productivityInsights = request.IncludeProductivityInsights 
                    ? GenerateProductivityInsights(tasks, analytics, zeitCoinMetrics)
                    : new List<ProductivityInsight>();

                // 7. Generate time patterns if requested
                TimePatterns? timePatterns = null;
                if (request.IncludeTimePatterns)
                {
                    timePatterns = GenerateTimePatterns(tasks, user);
                }

                // 8. Generate recommendations
                var recommendations = GenerateRecommendations(analytics, zeitCoinMetrics, timePatterns, productivityInsights);

                _logger.LogInformation("Analytics generated successfully for user {UserId}. {TaskCount} tasks analyzed", 
                    request.UserId, tasks.Count);

                return Result.SuccessResult(analytics, zeitCoinMetrics, productivityInsights, timePatterns, recommendations);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analytics for user {UserId}", request.UserId);
                return Result.Failure("An error occurred generating analytics. Please try again.");
            }
        }

        private (DateTime startDate, DateTime endDate) GetDateRange(TimeRange timeRange)
        {
            var now = DateTime.UtcNow;
            var endDate = now;

            var startDate = timeRange switch
            {
                TimeRange.Last7Days => now.AddDays(-7),
                TimeRange.Last30Days => now.AddDays(-30),
                TimeRange.Last90Days => now.AddDays(-90),
                TimeRange.ThisYear => new DateTime(now.Year, 1, 1),
                TimeRange.AllTime => DateTime.MinValue,
                _ => now.AddDays(-30)
            };

            return (startDate, endDate);
        }

        private UserAnalytics GenerateUserAnalytics(string userId, List<TaskDocument> tasks, string timeRangeStr, 
            DateTime startDate, DateTime endDate)
        {
            var completedTasks = tasks.Where(t => t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Completed).ToList();
            var totalTimeHours = completedTasks.Sum(t => (t.ActualMinutes ?? t.EstimatedMinutes ?? 0)) / 60.0;

            var categoryBreakdown = tasks
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => new CategoryStats
                {
                    TaskCount = g.Count(),
                    CompletedCount = g.Count(t => t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Completed),
                    TotalTimeHours = g.Where(t => t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Completed)
                                      .Sum(t => (t.ActualMinutes ?? t.EstimatedMinutes ?? 0)) / 60.0,
                    AverageProductivity = g.Where(t => t.Metadata?.ContainsKey("productivityRating") == true)
                                           .Average(t => Convert.ToDouble(t.Metadata["productivityRating"])),
                    AverageSatisfaction = g.Where(t => t.Metadata?.ContainsKey("satisfactionRating") == true)
                                           .Average(t => Convert.ToDouble(t.Metadata["satisfactionRating"])),
                    CompletionRate = g.Count() > 0 ? g.Count(t => t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Completed) / (double)g.Count() * 100 : 0
                });

            var dailyActivity = tasks
                .GroupBy(t => t.CreatedAt.Date)
                .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count());

            var productivityTrends = GenerateProductivityTrends(completedTasks, startDate, endDate);

            return new UserAnalytics
            {
                UserId = userId,
                TimeRange = timeRangeStr,
                TotalTasks = tasks.Count,
                CompletedTasks = completedTasks.Count,
                CompletionRate = tasks.Count > 0 ? completedTasks.Count / (double)tasks.Count * 100 : 0,
                TotalTimeSpentHours = totalTimeHours,
                AverageTaskDurationMinutes = completedTasks.Count > 0 ? 
                    completedTasks.Average(t => t.ActualMinutes ?? t.EstimatedMinutes ?? 0) : 0,
                AverageProductivityRating = completedTasks.Where(t => t.Metadata?.ContainsKey("productivityRating") == true)
                    .DefaultIfEmpty().Average(t => t?.Metadata != null && t.Metadata.ContainsKey("productivityRating") ? 
                        Convert.ToDouble(t.Metadata["productivityRating"]) : 0),
                AverageSatisfactionRating = completedTasks.Where(t => t.Metadata?.ContainsKey("satisfactionRating") == true)
                    .DefaultIfEmpty().Average(t => t?.Metadata != null && t.Metadata.ContainsKey("satisfactionRating") ? 
                        Convert.ToDouble(t.Metadata["satisfactionRating"]) : 0),
                TimeEstimationAccuracy = CalculateOverallTimeAccuracy(completedTasks),
                CategoryBreakdown = categoryBreakdown,
                DailyActivityCount = dailyActivity,
                ProductivityTrends = productivityTrends
            };
        }

        private double CalculateOverallTimeAccuracy(List<TaskDocument> completedTasks)
        {
            var accuracyValues = completedTasks
                .Where(t => t.Metadata?.ContainsKey("timeAccuracy") == true)
                .Select(t => Convert.ToDouble(t.Metadata["timeAccuracy"]))
                .ToList();

            return accuracyValues.Count > 0 ? accuracyValues.Average() : 0;
        }

        private List<ProductivityDataPoint> GenerateProductivityTrends(List<TaskDocument> completedTasks, 
            DateTime startDate, DateTime endDate)
        {
            var trends = new List<ProductivityDataPoint>();
            var current = startDate.Date;

            while (current <= endDate.Date)
            {
                var dayTasks = completedTasks.Where(t => t.CompletedAt?.Date == current).ToList();
                
                if (dayTasks.Any())
                {
                    trends.Add(new ProductivityDataPoint
                    {
                        Date = current,
                        TasksCompleted = dayTasks.Count,
                        TimeSpentHours = dayTasks.Sum(t => (t.ActualMinutes ?? 0)) / 60.0,
                        AverageProductivity = dayTasks.Where(t => t.Metadata?.ContainsKey("productivityRating") == true)
                            .DefaultIfEmpty().Average(t => t?.Metadata != null && t.Metadata.ContainsKey("productivityRating") ? 
                                Convert.ToDouble(t.Metadata["productivityRating"]) : 0),
                        AverageSatisfaction = dayTasks.Where(t => t.Metadata?.ContainsKey("satisfactionRating") == true)
                            .DefaultIfEmpty().Average(t => t?.Metadata != null && t.Metadata.ContainsKey("satisfactionRating") ? 
                                Convert.ToDouble(t.Metadata["satisfactionRating"]) : 0),
                        ZeitCoinPointsEarned = dayTasks.Where(t => t.IsZeitCoinEligible && 
                            t.Metadata?.ContainsKey("zeitCoinPoints") == true)
                            .Sum(t => Convert.ToInt32(t.Metadata["zeitCoinPoints"]))
                    });
                }

                current = current.AddDays(1);
            }

            return trends;
        }

        private ZeitCoinMetrics GenerateZeitCoinMetrics(List<TaskDocument> tasks)
        {
            var completedTasks = tasks.Where(t => t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Completed).ToList();
            var zeitCoinEligibleTasks = completedTasks.Where(t => t.IsZeitCoinEligible).ToList();
            
            var totalPoints = zeitCoinEligibleTasks
                .Where(t => t.Metadata?.ContainsKey("zeitCoinPoints") == true)
                .Sum(t => Convert.ToInt32(t.Metadata["zeitCoinPoints"]));

            var timeToValueValues = completedTasks
                .Where(t => t.Metadata?.ContainsKey("timeToValueRatio") == true)
                .Select(t => Convert.ToDouble(t.Metadata["timeToValueRatio"]))
                .ToList();

            var efficiencyValues = completedTasks
                .Where(t => t.Metadata?.ContainsKey("efficiencyScore") == true)
                .Select(t => Convert.ToDouble(t.Metadata["efficiencyScore"]))
                .ToList();

            var categoryEarnings = zeitCoinEligibleTasks
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, 
                    g => g.Where(t => t.Metadata?.ContainsKey("zeitCoinPoints") == true)
                          .Sum(t => Convert.ToInt32(t.Metadata["zeitCoinPoints"])));

            var projectedMonthly = totalPoints > 0 && tasks.Count > 0 ? 
                (int)(totalPoints * (30.0 / Math.Max(1, (DateTime.UtcNow - tasks.Min(t => t.CreatedAt)).TotalDays))) : 0;

            return new ZeitCoinMetrics
            {
                TotalPointsEarned = totalPoints,
                EligibleTasks = zeitCoinEligibleTasks.Count,
                EligibilityRate = completedTasks.Count > 0 ? zeitCoinEligibleTasks.Count / (double)completedTasks.Count * 100 : 0,
                AveragePointsPerTask = zeitCoinEligibleTasks.Count > 0 ? totalPoints / (double)zeitCoinEligibleTasks.Count : 0,
                ProjectedMonthlyEarnings = projectedMonthly,
                TimeToValueBaseline = timeToValueValues.Count > 0 ? timeToValueValues.Average() : 0,
                EfficiencyBaseline = efficiencyValues.Count > 0 ? efficiencyValues.Average() : 0,
                ValueCreationScore = CalculateValueCreationScore(completedTasks),
                OptimizationPotential = CalculateOptimizationPotential(completedTasks),
                CategoryEarnings = categoryEarnings
            };
        }

        private double CalculateValueCreationScore(List<TaskDocument> completedTasks)
        {
            var valueCreatedTasks = completedTasks.Count(t => 
                t.Metadata?.ContainsKey("valueCreated") == true && 
                !string.IsNullOrEmpty(t.Metadata["valueCreated"].ToString()));

            var totalTasks = completedTasks.Count;
            return totalTasks > 0 ? (valueCreatedTasks / (double)totalTasks) * 10 : 0;
        }

        private double CalculateOptimizationPotential(List<TaskDocument> completedTasks)
        {
            var lowEfficiencyTasks = completedTasks.Count(t => 
                t.Metadata?.ContainsKey("efficiencyScore") == true &&
                Convert.ToDouble(t.Metadata["efficiencyScore"]) < 5.0);

            var totalTasks = completedTasks.Count;
            return totalTasks > 0 ? (lowEfficiencyTasks / (double)totalTasks) * 100 : 0;
        }

        private List<ProductivityInsight> GenerateProductivityInsights(List<TaskDocument> tasks, 
            UserAnalytics analytics, ZeitCoinMetrics? zeitCoinMetrics)
        {
            var insights = new List<ProductivityInsight>();

            // Completion rate insights
            if (analytics.CompletionRate < 70)
            {
                insights.Add(new ProductivityInsight
                {
                    Type = "completion_rate",
                    Title = "Improve Task Completion",
                    Description = $"Your completion rate is {analytics.CompletionRate:F1}%. Consider breaking tasks into smaller, more manageable pieces.",
                    Impact = analytics.CompletionRate < 50 ? "high" : "medium",
                    Trend = "stable",
                    Data = new Dictionary<string, object> { {"completionRate", analytics.CompletionRate} }
                });
            }

            // Time estimation insights
            if (analytics.TimeEstimationAccuracy < 70)
            {
                insights.Add(new ProductivityInsight
                {
                    Type = "time_estimation",
                    Title = "Improve Time Estimation",
                    Description = $"Your time estimation accuracy is {analytics.TimeEstimationAccuracy:F1}%. Track actual vs estimated time to improve.",
                    Impact = "medium",
                    Trend = "stable",
                    Data = new Dictionary<string, object> { {"accuracy", analytics.TimeEstimationAccuracy} }
                });
            }

            // ZeitCoin insights
            if (zeitCoinMetrics != null && zeitCoinMetrics.EligibilityRate < 60)
            {
                insights.Add(new ProductivityInsight
                {
                    Type = "zeitcoin_eligibility",
                    Title = "Increase ZeitCoin Eligibility",
                    Description = $"Only {zeitCoinMetrics.EligibilityRate:F1}% of your completed tasks earn ZeitCoin. Focus on efficiency and value creation.",
                    Impact = "high",
                    Trend = "stable",
                    Data = new Dictionary<string, object> 
                    { 
                        {"eligibilityRate", zeitCoinMetrics.EligibilityRate},
                        {"totalPoints", zeitCoinMetrics.TotalPointsEarned}
                    }
                });
            }

            // Category balance insights
            var workPercentage = analytics.CategoryBreakdown.GetValueOrDefault("work", new CategoryStats()).TotalTimeHours / 
                                Math.Max(0.1, analytics.TotalTimeSpentHours) * 100;

            if (workPercentage > 80)
            {
                insights.Add(new ProductivityInsight
                {
                    Type = "work_life_balance",
                    Title = "Consider Work-Life Balance",
                    Description = $"{workPercentage:F1}% of your time is spent on work tasks. Consider adding personal and health activities.",
                    Impact = "medium",
                    Trend = "stable",
                    Data = new Dictionary<string, object> { {"workPercentage", workPercentage} }
                });
            }

            return insights;
        }

        private TimePatterns GenerateTimePatterns(List<TaskDocument> tasks, UserDocument user)
        {
            var completedTasks = tasks.Where(t => t.Status == SpinnerNet.Shared.Models.CosmosDb.TaskStatus.Completed).ToList();

            // Most productive hours (simplified analysis)
            var hourlyProductivity = completedTasks
                .Where(t => t.CompletedAt.HasValue && t.Metadata?.ContainsKey("productivityRating") == true)
                .GroupBy(t => t.CompletedAt!.Value.Hour)
                .ToDictionary(g => g.Key, g => g.Average(t => Convert.ToDouble(t.Metadata!["productivityRating"])));

            var mostProductiveHours = hourlyProductivity
                .OrderByDescending(kvp => kvp.Value)
                .Take(3)
                .Select(kvp => kvp.Key)
                .ToList();

            // Most productive days
            var dailyProductivity = completedTasks
                .Where(t => t.CompletedAt.HasValue && t.Metadata?.ContainsKey("productivityRating") == true)
                .GroupBy(t => t.CompletedAt!.Value.DayOfWeek)
                .ToDictionary(g => g.Key.ToString(), g => g.Average(t => Convert.ToDouble(t.Metadata!["productivityRating"])));

            var mostProductiveDays = dailyProductivity
                .OrderByDescending(kvp => kvp.Value)
                .Take(3)
                .Select(kvp => kvp.Key)
                .ToList();

            // Work-life balance
            var categoryTimes = tasks.GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.ActualMinutes ?? t.EstimatedMinutes ?? 0));

            var totalTime = Math.Max(1, categoryTimes.Values.Sum());
            var workLifeBalance = new WorkLifeBalanceMetrics
            {
                WorkHoursPercentage = categoryTimes.GetValueOrDefault("work", 0) / (double)totalTime * 100,
                PersonalHoursPercentage = (categoryTimes.GetValueOrDefault("personal", 0) + 
                                          categoryTimes.GetValueOrDefault("family", 0)) / (double)totalTime * 100,
                HealthHoursPercentage = categoryTimes.GetValueOrDefault("health", 0) / (double)totalTime * 100
            };

            workLifeBalance.BalanceScore = CalculateBalanceScore(workLifeBalance);
            workLifeBalance.Recommendations = GenerateBalanceRecommendations(workLifeBalance);

            return new TimePatterns
            {
                MostProductiveHours = mostProductiveHours,
                MostProductiveDays = mostProductiveDays,
                AverageSessionLength = completedTasks.Count > 0 ? 
                    completedTasks.Average(t => t.ActualMinutes ?? t.EstimatedMinutes ?? 0) : 0,
                WorkLifeBalance = workLifeBalance,
                SeasonalTrends = new Dictionary<string, double>(), // Simplified for Sprint 1
                PeakEfficiencyWindows = new List<TimeWindow>() // Simplified for Sprint 1
            };
        }

        private double CalculateBalanceScore(WorkLifeBalanceMetrics balance)
        {
            // Ideal distribution: 60% work, 25% personal, 15% health
            var workDeviation = Math.Abs(balance.WorkHoursPercentage - 60);
            var personalDeviation = Math.Abs(balance.PersonalHoursPercentage - 25);
            var healthDeviation = Math.Abs(balance.HealthHoursPercentage - 15);

            var totalDeviation = workDeviation + personalDeviation + healthDeviation;
            return Math.Max(0, 100 - totalDeviation);
        }

        private List<string> GenerateBalanceRecommendations(WorkLifeBalanceMetrics balance)
        {
            var recommendations = new List<string>();

            if (balance.WorkHoursPercentage > 70)
                recommendations.Add("Consider reducing work hours and adding personal time");

            if (balance.HealthHoursPercentage < 10)
                recommendations.Add("Increase health and fitness activities");

            if (balance.PersonalHoursPercentage < 15)
                recommendations.Add("Schedule more personal and family time");

            return recommendations;
        }

        private List<AnalyticsRecommendation> GenerateRecommendations(UserAnalytics analytics, 
            ZeitCoinMetrics? zeitCoinMetrics, TimePatterns? timePatterns, List<ProductivityInsight> insights)
        {
            var recommendations = new List<AnalyticsRecommendation>();

            // Time estimation improvement
            if (analytics.TimeEstimationAccuracy < 75)
            {
                recommendations.Add(new AnalyticsRecommendation
                {
                    Category = "time_management",
                    Title = "Improve Time Estimation Skills",
                    Description = "Track actual vs estimated time more consistently to improve planning accuracy",
                    Priority = "medium",
                    PotentialImpact = "Better planning and ZeitCoin eligibility",
                    TimeToImplement = "1-2 weeks",
                    Metadata = new Dictionary<string, object> { {"currentAccuracy", analytics.TimeEstimationAccuracy} }
                });
            }

            // ZeitCoin optimization
            if (zeitCoinMetrics != null && zeitCoinMetrics.EligibilityRate < 70)
            {
                recommendations.Add(new AnalyticsRecommendation
                {
                    Category = "zeitcoin",
                    Title = "Optimize for ZeitCoin Earnings",
                    Description = "Focus on efficiency and value creation to increase ZeitCoin eligibility",
                    Priority = "high",
                    PotentialImpact = $"Potential to increase monthly earnings to {zeitCoinMetrics.ProjectedMonthlyEarnings * 1.5:F0} points",
                    TimeToImplement = "2-3 weeks",
                    Metadata = new Dictionary<string, object> 
                    { 
                        {"currentEligibility", zeitCoinMetrics.EligibilityRate},
                        {"optimizationPotential", zeitCoinMetrics.OptimizationPotential}
                    }
                });
            }

            // Productivity patterns
            if (timePatterns?.MostProductiveHours.Any() == true)
            {
                recommendations.Add(new AnalyticsRecommendation
                {
                    Category = "productivity",
                    Title = "Optimize Task Scheduling",
                    Description = $"Schedule important tasks during your peak hours: {string.Join(", ", timePatterns.MostProductiveHours.Select(h => $"{h}:00"))}",
                    Priority = "medium",
                    PotentialImpact = "15-25% productivity improvement",
                    TimeToImplement = "Immediate",
                    Metadata = new Dictionary<string, object> { {"peakHours", timePatterns.MostProductiveHours} }
                });
            }

            return recommendations;
        }
    }

    // 6. Endpoint (HTTP API)
    [ApiController]
    [Route("api/analytics")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get comprehensive user analytics and time banking metrics
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="timeRange">Time range for analytics</param>
        /// <param name="includeZeitCoin">Include ZeitCoin metrics</param>
        /// <param name="includeInsights">Include productivity insights</param>
        /// <param name="includePatterns">Include time patterns</param>
        /// <param name="categories">Filter by specific categories</param>
        /// <returns>Comprehensive analytics with ZeitCoin foundation data</returns>
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        public async Task<ActionResult<Result>> GetUserAnalytics(
            [FromRoute] string userId,
            [FromQuery] TimeRange timeRange = TimeRange.Last30Days,
            [FromQuery] bool includeZeitCoin = true,
            [FromQuery] bool includeInsights = true,
            [FromQuery] bool includePatterns = true,
            [FromQuery] string[]? categories = null)
        {
            var command = new Command
            {
                UserId = userId,
                TimeRange = timeRange,
                IncludeZeitCoinMetrics = includeZeitCoin,
                IncludeProductivityInsights = includeInsights,
                IncludeTimePatterns = includePatterns,
                Categories = categories?.ToList()
            };

            var result = await _mediator.Send(command);

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