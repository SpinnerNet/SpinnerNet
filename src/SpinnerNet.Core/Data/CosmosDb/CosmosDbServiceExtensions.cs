using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SpinnerNet.Shared.Models.CosmosDb;

namespace SpinnerNet.Core.Data.CosmosDb;

/// <summary>
/// Service collection extensions for Cosmos DB integration
/// </summary>
public static class CosmosDbServiceExtensions
{
    /// <summary>
    /// Add Cosmos DB services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options using direct value extraction
        var configSection = configuration.GetSection(CosmosDbOptions.SectionName);
        services.Configure<CosmosDbOptions>(options =>
        {
            options.ConnectionString = configSection[nameof(CosmosDbOptions.ConnectionString)] ?? "";
            options.DatabaseName = configSection[nameof(CosmosDbOptions.DatabaseName)] ?? "SpinnerNet";
            options.DefaultMaxResults = int.TryParse(configSection[nameof(CosmosDbOptions.DefaultMaxResults)], out var maxResults) ? maxResults : 100;
            options.RetryAttempts = int.TryParse(configSection[nameof(CosmosDbOptions.RetryAttempts)], out var retryAttempts) ? retryAttempts : 3;
            options.RequestTimeoutSeconds = int.TryParse(configSection[nameof(CosmosDbOptions.RequestTimeoutSeconds)], out var timeout) ? timeout : 30;
            options.UseGatewayMode = bool.TryParse(configSection[nameof(CosmosDbOptions.UseGatewayMode)], out var useGateway) && useGateway;
            options.MaxConnectionsPerEndpoint = int.TryParse(configSection[nameof(CosmosDbOptions.MaxConnectionsPerEndpoint)], out var maxConnections) ? maxConnections : 50;
            if (int.TryParse(configSection[nameof(CosmosDbOptions.AutoscaleMaxThroughput)], out var autoscale))
                options.AutoscaleMaxThroughput = autoscale;
        });

        // Register Cosmos Client as singleton
        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
            
            var clientOptions = new CosmosClientOptions
            {
                ConnectionMode = options.UseGatewayMode ? ConnectionMode.Gateway : ConnectionMode.Direct,
                RequestTimeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds),
                MaxRequestsPerTcpConnection = options.MaxConnectionsPerEndpoint,
                MaxTcpConnectionsPerEndpoint = options.MaxConnectionsPerEndpoint,
                ConsistencyLevel = ConsistencyLevel.Session,
                AllowBulkExecution = true,
                EnableContentResponseOnWrite = false // Improve performance
            };

            return new CosmosClient(options.ConnectionString, clientOptions);
        });

        // Register repositories
        services.AddScoped<ICosmosRepository<UserDocument>, CosmosRepository<UserDocument>>();
        services.AddScoped<ICosmosRepository<PersonaDocument>, CosmosRepository<PersonaDocument>>();
        services.AddScoped<ICosmosRepository<BuddyDocument>, CosmosRepository<BuddyDocument>>();
        services.AddScoped<ICosmosRepository<EmailThreadDocument>, CosmosRepository<EmailThreadDocument>>();
        services.AddScoped<ICosmosRepository<BuddyMemoryDocument>, CosmosRepository<BuddyMemoryDocument>>();

        // Register unit of work
        services.AddScoped<ICosmosUnitOfWork, CosmosUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Initialize Cosmos DB database and containers
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>Task</returns>
    public static async Task InitializeCosmosDbAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

        // Create database
        var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
            options.DatabaseName,
            options.AutoscaleMaxThroughput.HasValue 
                ? ThroughputProperties.CreateAutoscaleThroughput(options.AutoscaleMaxThroughput.Value)
                : null);

        var database = databaseResponse.Database;

        // Create containers
        await CreateContainersAsync(database, options);
    }

    private static async Task CreateContainersAsync(Database database, CosmosDbOptions options)
    {
        var containers = new[]
        {
            new ContainerDefinition
            {
                Name = "Users",
                PartitionKeyPath = "/userId",
                DefaultTtl = null,
                IndexingPolicy = CreateUsersIndexingPolicy()
            },
            new ContainerDefinition
            {
                Name = "Personas",
                PartitionKeyPath = "/userId",
                DefaultTtl = null,
                IndexingPolicy = CreatePersonasIndexingPolicy()
            },
            new ContainerDefinition
            {
                Name = "EmailData",
                PartitionKeyPath = "/userId",
                DefaultTtl = 86400 * 365, // 1 year TTL
                IndexingPolicy = CreateEmailDataIndexingPolicy()
            },
            new ContainerDefinition
            {
                Name = "BuddyMemory",
                PartitionKeyPath = "/userId",
                DefaultTtl = 86400 * 30, // 30 days TTL
                IndexingPolicy = CreateBuddyMemoryIndexingPolicy()
            }
        };

        foreach (var containerDef in containers)
        {
            var containerProperties = new ContainerProperties(containerDef.Name, containerDef.PartitionKeyPath)
            {
                IndexingPolicy = containerDef.IndexingPolicy
            };

            if (containerDef.DefaultTtl.HasValue)
            {
                containerProperties.DefaultTimeToLive = containerDef.DefaultTtl.Value;
            }

            await database.CreateContainerIfNotExistsAsync(
                containerProperties,
                options.AutoscaleMaxThroughput.HasValue 
                    ? ThroughputProperties.CreateAutoscaleThroughput(options.AutoscaleMaxThroughput.Value)
                    : null);
        }
    }

    private static IndexingPolicy CreateUsersIndexingPolicy()
    {
        return new IndexingPolicy
        {
            IndexingMode = IndexingMode.Consistent,
            Automatic = true,
            IncludedPaths =
            {
                new IncludedPath { Path = "/email/?" },
                new IncludedPath { Path = "/isActive/?" },
                new IncludedPath { Path = "/createdAt/?" }
            },
            ExcludedPaths =
            {
                new ExcludedPath { Path = "/authProviders/*" },
                new ExcludedPath { Path = "/preferences/*" }
            }
        };
    }

    private static IndexingPolicy CreatePersonasIndexingPolicy()
    {
        return new IndexingPolicy
        {
            IndexingMode = IndexingMode.Consistent,
            Automatic = true,
            IncludedPaths =
            {
                new IncludedPath { Path = "/userId/?" },
                new IncludedPath { Path = "/isDefault/?" },
                new IncludedPath { Path = "/basicInfo/age/?" },
                new IncludedPath { Path = "/buddyRelationships/*/buddyId/?" },
                new IncludedPath { Path = "/buddyRelationships/*/isActive/?" }
            },
            ExcludedPaths =
            {
                new ExcludedPath { Path = "/typeLeapConfig/*" },
                new ExcludedPath { Path = "/privacySettings/*" }
            }
        };
    }

    private static IndexingPolicy CreateEmailDataIndexingPolicy()
    {
        return new IndexingPolicy
        {
            IndexingMode = IndexingMode.Consistent,
            Automatic = true,
            IncludedPaths =
            {
                new IncludedPath { Path = "/userId/?" },
                new IncludedPath { Path = "/threadInfo/isRead/?" },
                new IncludedPath { Path = "/aiAnalysis/category/?" },
                new IncludedPath { Path = "/aiAnalysis/priority/?" },
                new IncludedPath { Path = "/createdAt/?" },
                new IncludedPath { Path = "/messages/*/from/?" }
            },
            ExcludedPaths =
            {
                new ExcludedPath { Path = "/messages/*/bodyPreview/?" },
                new ExcludedPath { Path = "/messages/*/attachments/*" }
            }
        };
    }

    private static IndexingPolicy CreateBuddyMemoryIndexingPolicy()
    {
        return new IndexingPolicy
        {
            IndexingMode = IndexingMode.Consistent,
            Automatic = true,
            IncludedPaths =
            {
                new IncludedPath { Path = "/userId/?" },
                new IncludedPath { Path = "/buddyId/?" },
                new IncludedPath { Path = "/memoryType/?" },
                new IncludedPath { Path = "/lastUpdated/?" }
            },
            ExcludedPaths =
            {
                new ExcludedPath { Path = "/data/*" }
            }
        };
    }

    private class ContainerDefinition
    {
        public required string Name { get; set; }
        public required string PartitionKeyPath { get; set; }
        public int? DefaultTtl { get; set; }
        public required IndexingPolicy IndexingPolicy { get; set; }
    }
}

/// <summary>
/// Health check extensions for Cosmos DB
/// </summary>
public static class CosmosDbHealthCheckExtensions
{
    /// <summary>
    /// Add Cosmos DB health checks
    /// </summary>
    /// <param name="builder">Health checks builder</param>
    /// <param name="name">Health check name</param>
    /// <param name="tags">Health check tags</param>
    /// <returns>Health checks builder</returns>
    public static IHealthChecksBuilder AddCosmosDb(
        this IHealthChecksBuilder builder,
        string name = "cosmosdb",
        params string[] tags)
    {
        return builder.AddCheck<CosmosDbHealthCheck>(name, tags: tags);
    }
}

/// <summary>
/// Health check implementation for Cosmos DB
/// </summary>
public class CosmosDbHealthCheck : IHealthCheck
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosDbOptions _options;

    public CosmosDbHealthCheck(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options)
    {
        _cosmosClient = cosmosClient;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to read database properties to verify connection
            var database = _cosmosClient.GetDatabase(_options.DatabaseName);
            await database.ReadAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("Cosmos DB is responding");
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return HealthCheckResult.Unhealthy($"Cosmos DB database '{_options.DatabaseName}' not found", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cosmos DB is not responding", ex);
        }
    }
}