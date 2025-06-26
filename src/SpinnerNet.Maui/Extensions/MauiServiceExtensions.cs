using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Core.Extensions;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Reflection;

namespace SpinnerNet.Maui.Extensions;

/// <summary>
/// MAUI-specific service extensions that exclude web components
/// </summary>
public static class MauiServiceExtensions
{
    /// <summary>
    /// Add vertical slice architecture services for MAUI (without controllers)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMauiVerticalSliceServices(this IServiceCollection services)
    {
        // Add MediatR for command/query handling
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(SpinnerNet.Core.Features.Users.RegisterUser))!);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(SpinnerNet.Core.Features.Users.RegisterUser))!);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    /// <summary>
    /// Add Cosmos DB repositories for MAUI (optional cloud sync)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">Cosmos DB connection string</param>
    /// <param name="databaseName">Database name</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMauiCosmosDbRepositories(this IServiceCollection services, 
        string connectionString, 
        string databaseName)
    {
        // Configure Cosmos DB options
        services.Configure<CosmosDbOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.DatabaseName = databaseName;
        });

        // Add Cosmos DB client
        services.AddSingleton<Microsoft.Azure.Cosmos.CosmosClient>(provider =>
        {
            return new Microsoft.Azure.Cosmos.CosmosClient(connectionString);
        });

        // Register Cosmos DB repositories without controller dependencies
        services.AddSingleton<ICosmosRepository<UserDocument>>(provider =>
        {
            var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>();
            var logger = provider.GetRequiredService<ILogger<CosmosRepository<UserDocument>>>();
            return new CosmosRepository<UserDocument>(cosmosClient, options, logger);
        });

        services.AddSingleton<ICosmosRepository<PersonaDocument>>(provider =>
        {
            var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>();
            var logger = provider.GetRequiredService<ILogger<CosmosRepository<PersonaDocument>>>();
            return new CosmosRepository<PersonaDocument>(cosmosClient, options, logger);
        });

        // COMMENTED OUT FOR SPRINT 1 - BUDDY FEATURES ARE FUTURE SPRINT
        // services.AddSingleton<ICosmosRepository<BuddyDocument>>(provider =>
        // {
        //     var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
        //     var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>();
        //     var logger = provider.GetRequiredService<ILogger<CosmosRepository<BuddyDocument>>>();
        //     return new CosmosRepository<BuddyDocument>(cosmosClient, options, logger);
        // });

        // COMMENTED OUT FOR SPRINT 1 - TASK FEATURES ARE FUTURE SPRINT
        // services.AddSingleton<ICosmosRepository<TaskDocument>>(provider =>
        // {
        //     var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
        //     var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>();
        //     var logger = provider.GetRequiredService<ILogger<CosmosRepository<TaskDocument>>>();
        //     return new CosmosRepository<TaskDocument>(cosmosClient, options, logger);
        // });

        // COMMENTED OUT FOR SPRINT 1 - GOAL FEATURES ARE FUTURE SPRINT
        // services.AddSingleton<ICosmosRepository<GoalDocument>>(provider =>
        // {
        //     var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
        //     var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>();
        //     var logger = provider.GetRequiredService<ILogger<CosmosRepository<GoalDocument>>>();
        //     return new CosmosRepository<GoalDocument>(cosmosClient, options, logger);
        // });

        services.AddSingleton<ICosmosRepository<BuddyMemoryDocument>>(provider =>
        {
            var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>();
            var logger = provider.GetRequiredService<ILogger<CosmosRepository<BuddyMemoryDocument>>>();
            return new CosmosRepository<BuddyMemoryDocument>(cosmosClient, options, logger);
        });

        services.AddSingleton<ICosmosRepository<InterviewSessionDocument>>(provider =>
        {
            var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>();
            var logger = provider.GetRequiredService<ILogger<CosmosRepository<InterviewSessionDocument>>>();
            return new CosmosRepository<InterviewSessionDocument>(cosmosClient, options, logger);
        });

        services.AddSingleton<ICosmosRepository<ConversationDocument>>(provider =>
        {
            var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>();
            var logger = provider.GetRequiredService<ILogger<CosmosRepository<ConversationDocument>>>();
            return new CosmosRepository<ConversationDocument>(cosmosClient, options, logger);
        });

        return services;
    }
}