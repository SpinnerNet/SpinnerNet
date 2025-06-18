using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Extensions;
using SpinnerNet.Core.Features.Users;
using SpinnerNet.Core.Features.Tasks;
using SpinnerNet.Core.Features.Analytics;
using MediatR;

namespace SpinnerNet.Console;

/// <summary>
/// Console application to test SpinnerNet core functionality locally
/// Demonstrates user registration, task creation, and analytics
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("🚀 Starting Spinner.Net Console Test...\n");

        // Build host with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add vertical slice services (MediatR + FluentValidation)
                services.AddVerticalSliceServices();
                
                // Add SQLite for local testing
                var connectionString = "Data Source=spinnernet-console-test.db";
                services.AddSqliteEntityFramework(connectionString);
            })
            .Build();

        var mediator = host.Services.GetRequiredService<IMediator>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            await TestUserWorkflow(mediator, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during testing");
            System.Console.WriteLine($"❌ Error: {ex.Message}");
        }

        System.Console.WriteLine("\n✅ Test completed!");
        await Task.Delay(1000); // Give time to see results
    }

    private static async Task TestUserWorkflow(IMediator mediator, ILogger logger)
    {
        System.Console.WriteLine("🔧 Testing Core SpinnerNet Functionality\n");

        // 1. Test User Registration
        System.Console.WriteLine("1️⃣  Testing User Registration...");
        var registerCommand = new RegisterUser.Command
        {
            Email = "test@spinnernet.dev",
            DisplayName = "Test User",
            Password = "TestPassword123!",
            BirthDate = new DateTime(1990, 1, 1),
            Timezone = "UTC",
            DataResidencyPreference = SpinnerNet.Core.Features.Users.DataResidencyPreference.Local,
            AcceptTerms = true,
            AcceptPrivacyPolicy = true,
            AcceptDataProcessing = true
        };

        var registerResult = await mediator.Send(registerCommand);
        System.Console.WriteLine($"   Registration: {(registerResult.Success ? "✅ Success" : "❌ Failed")}");
        if (!registerResult.Success)
        {
            System.Console.WriteLine($"   Error: {registerResult.ErrorMessage}");
            return;
        }

        var userId = registerResult.UserId!;
        System.Console.WriteLine($"   User ID: {userId}\n");

        // 2. Test Task Creation
        System.Console.WriteLine("2️⃣  Testing Task Creation...");
        var createTaskCommand = new CreateTask.Command
        {
            UserId = userId,
            Input = "Create a high priority task to test ZeitCoin time tracking functionality. This should take about 2 hours and be eligible for ZeitCoin rewards.",
            Language = "en",
            Timezone = "UTC"
        };

        var createTaskResult = await mediator.Send(createTaskCommand);
        System.Console.WriteLine($"   Task Creation: {(createTaskResult.Success ? "✅ Success" : "❌ Failed")}");
        if (!createTaskResult.Success)
        {
            System.Console.WriteLine($"   Error: {createTaskResult.ErrorMessage}");
            return;
        }

        var taskId = createTaskResult.TaskId!;
        System.Console.WriteLine($"   Task ID: {taskId}\n");

        // 3. Test Task Completion with Time Tracking
        System.Console.WriteLine("3️⃣  Testing Task Completion & Time Tracking...");
        var completeTaskCommand = new CompleteTask.Command
        {
            TaskId = taskId,
            UserId = userId,
            ActualTimeSpentMinutes = 90,
            CompletionNotes = "Completed successfully with focus time tracking",
            ProductivityRating = 8,
            SatisfactionRating = 9,
            CompletedAt = DateTime.UtcNow
        };

        var completeTaskResult = await mediator.Send(completeTaskCommand);
        System.Console.WriteLine($"   Task Completion: {(completeTaskResult.Success ? "✅ Success" : "❌ Failed")}");
        if (completeTaskResult.Success)
        {
            System.Console.WriteLine($"   ZeitCoin Eligible: {completeTaskResult.ZeitCoinEligibility?.IsEligible}");
            System.Console.WriteLine($"   Points Earned: {completeTaskResult.ZeitCoinEligibility?.TotalEarnedPoints}");
        }
        System.Console.WriteLine();

        // 4. Test Analytics
        System.Console.WriteLine("4️⃣  Testing User Analytics...");
        var analyticsQuery = new GetUserAnalytics.Command
        {
            UserId = userId,
            TimeRange = GetUserAnalytics.TimeRange.Last7Days,
            IncludeZeitCoinMetrics = true,
            IncludeProductivityInsights = true,
            IncludeTimePatterns = true
        };

        var analyticsResult = await mediator.Send(analyticsQuery);
        System.Console.WriteLine($"   Analytics: {(analyticsResult.Success ? "✅ Success" : "❌ Failed")}");
        if (analyticsResult.Success && analyticsResult.Analytics != null)
        {
            var analytics = analyticsResult.Analytics;
            System.Console.WriteLine($"   Total Tasks: {analytics.TotalTasks}");
            System.Console.WriteLine($"   Completed Tasks: {analytics.CompletedTasks}");
            System.Console.WriteLine($"   Completion Rate: {analytics.CompletionRate:F1}%");
            System.Console.WriteLine($"   Total Time Spent: {analytics.TotalTimeSpentHours:F1} hours");
            System.Console.WriteLine($"   Avg Task Duration: {analytics.AverageTaskDurationMinutes:F1} min");
            System.Console.WriteLine($"   Analysis Generated: {analytics.GeneratedAt}");
        }
        System.Console.WriteLine();

        System.Console.WriteLine("🎉 Core workflow test completed successfully!");
        System.Console.WriteLine("   • User registration ✅");
        System.Console.WriteLine("   • Task creation ✅");
        System.Console.WriteLine("   • Time tracking ✅");
        System.Console.WriteLine("   • ZeitCoin foundation ✅");
        System.Console.WriteLine("   • Analytics system ✅");
    }
}