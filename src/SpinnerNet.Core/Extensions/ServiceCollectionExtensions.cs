using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Reflection;

namespace SpinnerNet.Core.Extensions;

/// <summary>
/// Extension methods for configuring services in the DI container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add vertical slice architecture services (MediatR + FluentValidation)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddVerticalSliceServices(this IServiceCollection services)
    {
        // Add MediatR for command/query handling
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    /// <summary>
    /// Add Cosmos DB repositories for vertical slices
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">Cosmos DB connection string</param>
    /// <param name="databaseName">Database name</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCosmosDbRepositories(this IServiceCollection services, 
        string connectionString, 
        string databaseName)
    {
        // Configure Cosmos DB options
        services.Configure<CosmosDbOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.DatabaseName = databaseName;
        });

        // Register Cosmos DB repositories with proper dependency injection
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

        services.AddSingleton<ICosmosRepository<GoalDocument>>(provider =>
        {
            var cosmosClient = provider.GetRequiredService<Microsoft.Azure.Cosmos.CosmosClient>();
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<CosmosDbOptions>>();
            var logger = provider.GetRequiredService<ILogger<CosmosRepository<GoalDocument>>>();
            return new CosmosRepository<GoalDocument>(cosmosClient, options, logger);
        });

        return services;
    }

    /*
    /// <summary>
    /// Add SQLite Entity Framework for Sprint 1 development
    /// COMMENTED OUT FOR SPRINT 1 - Using Cosmos DB instead
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">SQLite connection string</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSqliteEntityFramework(this IServiceCollection services, 
        string connectionString)
    {
        // Add Entity Framework with SQLite
        services.AddDbContext<SpinnerNetDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }
    */
}

/// <summary>
/// MediatR pipeline behavior for validation
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}

/// <summary>
/// MediatR pipeline behavior for logging
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            _logger.LogInformation("Successfully handled {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName}", requestName);
            throw;
        }
    }
}