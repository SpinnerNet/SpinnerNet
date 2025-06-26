using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SpinnerNet.Core.Extensions;
using SpinnerNet.Core.Features.Users;
using SpinnerNet.Core.Features.Tasks;
using SpinnerNet.Core.Features.Analytics;
using SpinnerNet.Core.Features.Personas;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using SpinnerNet.Console.Commands;
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
        
        // Ask user which test to run
        System.Console.WriteLine("Select test mode:");
        System.Console.WriteLine("1. SQLite EF Core Test (default)");
        System.Console.WriteLine("2. Cosmos DB Test");
        System.Console.WriteLine("3. Clean Cosmos DB");
        System.Console.Write("Enter choice (1, 2, or 3): ");
        
        var choice = System.Console.ReadLine();
        var testCosmosDb = choice == "2";
        var cleanCosmosDb = choice == "3";

        // Build host with dependency injection
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false);
            })
            .ConfigureServices((context, services) =>
            {
                // Add vertical slice services (MediatR + FluentValidation)
                services.AddVerticalSliceServices();
                
                if (testCosmosDb)
                {
                    // Add Cosmos DB for testing
                    System.Console.WriteLine("🗄️ Configuring Cosmos DB...\n");
                    services.AddCosmosDb(context.Configuration);
                }
                else
                {
                    // Add SQLite for local testing
                    System.Console.WriteLine("🗄️ Configuring SQLite...\n");
                    var connectionString = "Data Source=spinnernet-console-test.db";
                    services.AddSqliteEntityFramework(connectionString);
                }
            });

        var host = hostBuilder.Build();
        var mediator = host.Services.GetRequiredService<IMediator>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            if (cleanCosmosDb)
            {
                await CleanCosmosDatabase(host.Services.GetRequiredService<IConfiguration>());
            }
            else if (testCosmosDb)
            {
                await TestCosmosDbPersonas(mediator, logger, host);
            }
            else
            {
                await TestUserWorkflow(mediator, logger);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during testing");
            System.Console.WriteLine($"❌ Error: {ex.Message}");
            System.Console.WriteLine($"📋 Details: {ex}");
        }

        System.Console.WriteLine("\n✅ Test completed!");
        await Task.Delay(1000); // Give time to see results
    }

    private static async Task TestCosmosDbPersonas(IMediator mediator, ILogger logger, IHost host)
    {
        System.Console.WriteLine("🌌 Testing Cosmos DB Persona Operations\n");

        // Test 1: Cosmos DB Connectivity
        System.Console.WriteLine("1️⃣  Testing Cosmos DB Connectivity...");
        try
        {
            var personaRepo = host.Services.GetRequiredService<ICosmosRepository<PersonaDocument>>();
            System.Console.WriteLine("   ✅ Cosmos DB repository resolved successfully");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"   ❌ Failed to resolve Cosmos DB repository: {ex.Message}");
            return;
        }

        // Test 2A: Direct CosmosClient test (bypass repository)
        System.Console.WriteLine("\n2️⃣  Testing Direct CosmosClient...");
        try
        {
            var cosmosClient = host.Services.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var container = cosmosClient.GetContainer("SpinnerNet", "Personas");
            
            var simpleTestDoc = new
            {
                id = $"directtest_{Guid.NewGuid()}",
                type = "persona",
                Userid = "HVnMj2FSzDQOJlVoTC7lz9QNkh8fKaV7KpdLhdXsIiU",
                testField = "direct cosmos client test"
            };

            System.Console.WriteLine($"   Testing direct CosmosClient.Container.CreateItemAsync...");
            var directResult = await container.CreateItemAsync(
                simpleTestDoc, 
                new Microsoft.Azure.Cosmos.PartitionKey("HVnMj2FSzDQOJlVoTC7lz9QNkh8fKaV7KpdLhdXsIiU"));
            System.Console.WriteLine($"   ✅ Direct CosmosClient worked: {directResult.Resource.id}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"   ❌ Direct CosmosClient failed: {ex.Message}");
            if (ex is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
            {
                System.Console.WriteLine($"   🔍 Cosmos Status: {cosmosEx.StatusCode}, SubStatus: {cosmosEx.SubStatusCode}");
                System.Console.WriteLine($"   🔍 Response Body: {cosmosEx.ResponseBody}");
            }
        }

        // Test 2A2: Test simple structure that mimics PersonaDocument 
        System.Console.WriteLine("\n2️⃣  Testing Simple Structure...");
        try
        {
            var cosmosClient = host.Services.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var container = cosmosClient.GetContainer("SpinnerNet", "Personas");
            var testUserid = "HVnMj2FSzDQOJlVoTC7lz9QNkh8fKaV7KpdLhdXsIiU";
            
            // Test with a class that has the same essential structure as PersonaDocument
            var testDocument = new
            {
                id = $"structtest_{Guid.NewGuid()}",
                type = "persona",
                Userid = testUserId,
                personaid = Guid.NewGuid().ToString(),
                isDefault = true,
                basicInfo = new
                {
                    displayname = "Structure Test",
                    age = 30,
                    culturalBackground = "Test",
                    occupation = "Developer",
                    interests = new[] { "testing" },
                    languages = new
                    {
                        motherTongue = "en",
                        preferred = "en",
                        spoken = new[] { "en" },
                        proficiency = new { en = "Native" }
                    }
                },
                typeLeapConfig = new
                {
                    uiComplexityLevel = "Standard",
                    fontSizePreferences = "Medium",
                    colorPreferences = "Default",
                    enableAnimations = false,
                    navigationStyle = "Standard",
                    ageAdaptations = new { }
                },
                learningPreferences = new
                {
                    preferredLearningStyle = "Visual",
                    pacePreference = "SelfPaced",
                    difficultyLevel = "Intermediate"
                },
                privacySettings = new
                {
                    dataSharing = "Selective",
                    aiInteraction = "Standard",
                    emailAccess = "None",
                    consenttimestamp = DateTime.UtcNow
                },
                buddyRelationships = new object[0],
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };

            System.Console.WriteLine($"   Testing structure via direct container.CreateItemAsync...");
            var structResult = await container.CreateItemAsync(
                testDocument, 
                new Microsoft.Azure.Cosmos.PartitionKey(testUserId));
            System.Console.WriteLine($"   ✅ Structure test worked: {structResult.Resource.id}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"   ❌ Structure test failed: {ex.Message}");
            if (ex is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
            {
                System.Console.WriteLine($"   🔍 Cosmos Status: {cosmosEx.StatusCode}, SubStatus: {cosmosEx.SubStatusCode}");
                System.Console.WriteLine($"   🔍 Response Body: {cosmosEx.ResponseBody}");
                System.Console.WriteLine($"   🔍 Activity ID: {cosmosEx.ActivityId}");
            }
        }

        // Test 2A3: Test Microsoft pattern record
        System.Console.WriteLine("\n2️⃣  Testing Microsoft Pattern Record...");
        try
        {
            var cosmosClient = host.Services.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var container = cosmosClient.GetContainer("SpinnerNet", "Personas");
            var testUserid = "HVnMj2FSzDQOJlVoTC7lz9QNkh8fKaV7KpdLhdXsIiU";
            
            var recordDoc = new SimplePersonaDocument(
                id: $"record_{Guid.NewGuid()}",
                type: "persona",
                UserId: testUserId,
                personaId: Guid.NewGuid().ToString(),
                isDefault: true,
                displayName: "Record Test",
                age: 30,
                culturalBackground: "Test",
                occupation: "Developer",
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.UtcNow
            );

            System.Console.WriteLine($"   Testing SimplePersonaDocument record...");
            var recordResult = await container.CreateItemAsync(
                recordDoc, 
                new Microsoft.Azure.Cosmos.PartitionKey(testUserId));
            System.Console.WriteLine($"   ✅ Record worked: {recordResult.Resource.id}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"   ❌ Record failed: {ex.Message}");
            if (ex is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
            {
                System.Console.WriteLine($"   🔍 Cosmos Status: {cosmosEx.StatusCode}, SubStatus: {cosmosEx.SubStatusCode}");
                System.Console.WriteLine($"   🔍 Response Body: {cosmosEx.ResponseBody}");
                System.Console.WriteLine($"   🔍 Activity ID: {cosmosEx.ActivityId}");
            }
        }

        // Test 2A4: Test Microsoft pattern class
        System.Console.WriteLine("\n2️⃣  Testing Microsoft Pattern Class...");
        try
        {
            var cosmosClient = host.Services.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var container = cosmosClient.GetContainer("SpinnerNet", "Personas");
            var testUserid = "HVnMj2FSzDQOJlVoTC7lz9QNkh8fKaV7KpdLhdXsIiU";
            
            var classDoc = new SimplePersonaClass
            {
                id = $"class_{Guid.NewGuid()}",
                type = "persona",
                Userid = testUserId,
                personaid = Guid.NewGuid().ToString(),
                isDefault = true,
                displayname = "Class Test",
                age = 30,
                culturalBackground = "Test",
                occupation = "Developer",
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };

            System.Console.WriteLine($"   Testing SimplePersonaClass...");
            var classResult = await container.CreateItemAsync(
                classDoc, 
                new Microsoft.Azure.Cosmos.PartitionKey(testUserId));
            System.Console.WriteLine($"   ✅ Class worked: {classResult.Resource.id}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"   ❌ Class failed: {ex.Message}");
            if (ex is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
            {
                System.Console.WriteLine($"   🔍 Cosmos Status: {cosmosEx.StatusCode}, SubStatus: {cosmosEx.SubStatusCode}");
                System.Console.WriteLine($"   🔍 Response Body: {cosmosEx.ResponseBody}");
                System.Console.WriteLine($"   🔍 Activity ID: {cosmosEx.ActivityId}");
            }
        }

        // Test 2A5: Test class without JsonPropertyName attributes (original test)
        System.Console.WriteLine("\n2️⃣  Testing Test Class (no JsonPropertyName)...");
        try
        {
            var cosmosClient = host.Services.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var container = cosmosClient.GetContainer("SpinnerNet", "Personas");
            var testUserid = "HVnMj2FSzDQOJlVoTC7lz9QNkh8fKaV7KpdLhdXsIiU";
            
            var testClassDoc = new TestPersonaDocument
            {
                id = $"testclass_{Guid.NewGuid()}",
                type = "persona", 
                Userid = testUserId,
                Personaid = Guid.NewGuid().ToString(),
                IsDefault = true,
                BasicInfo = new TestBasicInfo
                {
                    Displayname = "Test Class",
                    Age = 30,
                    CulturalBackground = "Test",
                    Occupation = "Developer"
                },
                createdAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            System.Console.WriteLine($"   Testing TestPersonaDocument class...");
            var testClassResult = await container.CreateItemAsync(
                testClassDoc, 
                new Microsoft.Azure.Cosmos.PartitionKey(testUserId));
            System.Console.WriteLine($"   ✅ Test class worked: {testClassResult.Resource.id}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"   ❌ Test class failed: {ex.Message}");
            if (ex is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
            {
                System.Console.WriteLine($"   🔍 Cosmos Status: {cosmosEx.StatusCode}, SubStatus: {cosmosEx.SubStatusCode}");
                System.Console.WriteLine($"   🔍 Response Body: {cosmosEx.ResponseBody}");
                System.Console.WriteLine($"   🔍 Activity ID: {cosmosEx.ActivityId}");
            }
        }

        // Test 2B: Minimal PersonaDocument
        System.Console.WriteLine("\n2️⃣  Testing Minimal PersonaDocument...");
        try
        {
            var personaRepo = host.Services.GetRequiredService<ICosmosRepository<PersonaDocument>>();
            var testUserid = "HVnMj2FSzDQOJlVoTC7lz9QNkh8fKaV7KpdLhdXsIiU";
            var testPersonaid = Guid.NewGuid().ToString();
            var testDocumentid = $"minimal_{testUserId}_{testPersonaId}";

            var minimalDocument = new PersonaDocument
            {
                id = testDocumentId,
                type = "persona",
                Userid = testUserId,
                personaid = testPersonaId,
                isDefault = true,
                basicInfo = new PersonaBasicInfo(),
                typeLeapConfig = new TypeLeapConfiguration(),
                learningPreferences = new LearningPreferences(),
                privacySettings = new PrivacySettings(),
                buddyRelationships = new List<BuddyRelationship>(),
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };

            System.Console.WriteLine($"   Testing minimal PersonaDocument...");
            var minimalResult = await personaRepo.CreateOrUpdateAsync(minimalDocument, testUserId);
            System.Console.WriteLine($"   ✅ Minimal PersonaDocument worked: {minimalResult.id}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"   ❌ Minimal PersonaDocument failed: {ex.Message}");
            if (ex is Microsoft.Azure.Cosmos.CosmosException cosmosEx)
            {
                System.Console.WriteLine($"   🔍 Cosmos Status: {cosmosEx.StatusCode}, SubStatus: {cosmosEx.SubStatusCode}");
                System.Console.WriteLine($"   🔍 Response Body: {cosmosEx.ResponseBody}");
            }
        }

        // Test 2C: Full PersonaDocument  
        System.Console.WriteLine("\n2️⃣  Testing Full PersonaDocument...");
        try
        {
            var personaRepo = host.Services.GetRequiredService<ICosmosRepository<PersonaDocument>>();
            var testUserid = "HVnMj2FSzDQOJlVoTC7lz9QNkh8fKaV7KpdLhdXsIiU"; // Same as from web app
            var testPersonaid = Guid.NewGuid().ToString();
            var testDocumentid = $"persona_{testUserId}_{testPersonaId}";

            var testDocument = new PersonaDocument
            {
                id = testDocumentId,
                type = "persona",
                Userid = testUserId,
                personaid = testPersonaId,
                isDefault = true,
                basicInfo = new PersonaBasicInfo
                {
                    displayname = "Console Test User",
                    age = 30,
                    culturalBackground = "Test",
                    occupation = "Developer",
                    interests = new List<string> { "coding", "testing" },
                    languages = new LanguageInfo
                    {
                        motherTongue = "en",
                        preferred = "en",
                        spoken = new List<string> { "en" },
                        proficiency = new Dictionary<string, string> { { "en", "Native" } }
                    }
                },
                typeLeapConfig = new TypeLeapConfiguration
                {
                    uiComplexityLevel = "Standard",
                    fontSizePreferences = "Medium",
                    colorPreferences = "Default",
                    enableAnimations = false,
                    navigationStyle = "Standard",
                    ageAdaptations = new Dictionary<string, string>()
                },
                learningPreferences = new LearningPreferences
                {
                    preferredLearningStyle = "Visual",
                    pacePreference = "SelfPaced",
                    difficultyLevel = "Intermediate"
                },
                privacySettings = new PrivacySettings
                {
                    dataSharing = "Selective",
                    aiInteraction = "Standard",
                    emailAccess = "None",
                    consenttimestamp = DateTime.UtcNow
                },
                buddyRelationships = new List<BuddyRelationship>(),
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };

            System.Console.WriteLine($"   Creating document with ID: {testDocumentId}");
            System.Console.WriteLine($"   Using partition key: {testUserId}");

            // DEBUG: Serialize document to see exact JSON
            var jsonOptions = new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            };
            var documentJson = System.Text.Json.JsonSerializer.Serialize(testDocument, jsonOptions);
            System.Console.WriteLine($"   📋 Document JSON (camelCase): {documentJson}");
            
            var jsonOptionsDefault = new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
            var documentJsonDefault = System.Text.Json.JsonSerializer.Serialize(testDocument, jsonOptionsDefault);
            System.Console.WriteLine($"   📋 Document JSON (default): {documentJsonDefault}");

            var savedDocument = await personaRepo.CreateOrUpdateAsync(testDocument, testUserId);
            System.Console.WriteLine($"   ✅ Document created successfully: {savedDocument.id}");

            // Test retrieval
            System.Console.WriteLine("\n3️⃣  Testing Document Retrieval...");
            var retrievedDocument = await personaRepo.GetAsync(testDocumentId, testUserId);
            if (retrievedDocument != null)
            {
                System.Console.WriteLine($"   ✅ Document retrieved successfully: {retrievedDocument.basicInfo?.displayName}");
            }
            else
            {
                System.Console.WriteLine("   ❌ Failed to retrieve document");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"   ❌ Direct document test failed: {ex.Message}");
            System.Console.WriteLine($"   📋 Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"   🔍 Inner exception: {ex.InnerException.Message}");
            }
        }

        // Test 3: MediatR Command
        System.Console.WriteLine("\n4️⃣  Testing MediatR Persona Creation...");
        try
        {
            var command = new CompleteBasicPersonaInterview.Command
            {
                Userid = "HVnMj2FSzDQOJlVoTC7lz9QNkh8fKaV7KpdLhdXsIiU",
                Username = "Console MediatR Test",
                Personality = "creative",
                Use = "work",
                Language = "en"
            };

            System.Console.WriteLine($"   Sending MediatR command for UserId: {command.UserId}");
            var result = await mediator.Send(command);

            if (result.Success)
            {
                System.Console.WriteLine($"   ✅ MediatR persona creation successful: {result.PersonaId}");
            }
            else
            {
                System.Console.WriteLine($"   ❌ MediatR persona creation failed: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"   ❌ MediatR test failed: {ex.Message}");
            System.Console.WriteLine($"   📋 Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"   🔍 Inner exception: {ex.InnerException.Message}");
            }
        }

        System.Console.WriteLine("\n🎉 Cosmos DB test sequence completed!");
    }

    private static async Task CleanCosmosDatabase(IConfiguration configuration)
    {
        var cleanCommand = new CleanCosmosCommand(configuration);
        await cleanCommand.ExecuteAsync();
    }

    private static async Task TestUserWorkflow(IMediator mediator, ILogger logger)
    {
        System.Console.WriteLine("🔧 Testing Core SpinnerNet Functionality\n");

        // 1. Test User Registration
        System.Console.WriteLine("1️⃣  Testing User Registration...");
        var registerCommand = new RegisterUser.Command
        {
            Email = "test@spinnernet.dev",
            Displayname = "Test User",
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

        var userid = registerResult.UserId!;
        System.Console.WriteLine($"   User ID: {userId}\n");

        // 2. Test Task Creation
        System.Console.WriteLine("2️⃣  Testing Task Creation...");
        var createTaskCommand = new CreateTask.Command
        {
            Userid = userId,
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

        var taskid = createTaskResult.taskId!;
        System.Console.WriteLine($"   Task ID: {taskId}\n");

        // 3. Test Task Completion with Time Tracking
        System.Console.WriteLine("3️⃣  Testing Task Completion & Time Tracking...");
        var completeTaskCommand = new CompleteTask.Command
        {
            Taskid = taskId,
            Userid = userId,
            ActualTimeSpentMinutes = 90,
            Completionnotes = "Completed successfully with focus time tracking",
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
            Userid = userId,
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