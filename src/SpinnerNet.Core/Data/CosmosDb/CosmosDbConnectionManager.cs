using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace SpinnerNet.Core.Data.CosmosDb;

/// <summary>
/// Manages Cosmos DB connections and container references with connection pooling
/// </summary>
public interface ICosmosDbConnectionManager
{
    /// <summary>
    /// Get a container instance for the specified document type
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    /// <returns>Container instance</returns>
    Container GetContainer<T>() where T : class;

    /// <summary>
    /// Get a container instance by name
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <returns>Container instance</returns>
    Container GetContainer(string containerName);

    /// <summary>
    /// Get the Cosmos DB client
    /// </summary>
    /// <returns>Cosmos client instance</returns>
    CosmosClient GetClient();

    /// <summary>
    /// Get the database instance
    /// </summary>
    /// <returns>Database instance</returns>
    Database GetDatabase();

    /// <summary>
    /// Test connection to Cosmos DB
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection is successful</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get connection statistics
    /// </summary>
    /// <returns>Connection statistics</returns>
    CosmosDbConnectionStats GetConnectionStats();
}

/// <summary>
/// Implementation of Cosmos DB connection manager
/// </summary>
public class CosmosDbConnectionManager : ICosmosDbConnectionManager, IDisposable
{
    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly CosmosDbOptions _options;
    private readonly ILogger<CosmosDbConnectionManager> _logger;
    private readonly ConcurrentDictionary<string, Container> _containerCache = new();
    private readonly CosmosDbConnectionStats _connectionStats = new();
    private readonly Timer _statsTimer;

    public CosmosDbConnectionManager(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        ILogger<CosmosDbConnectionManager> logger)
    {
        _cosmosClient = cosmosClient;
        _options = options.Value;
        _logger = logger;
        _database = _cosmosClient.GetDatabase(_options.DatabaseName);

        // Start stats collection timer
        _statsTimer = new Timer(CollectStats, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

        _logger.LogInformation("Cosmos DB connection manager initialized for database '{DatabaseName}'", _options.DatabaseName);
    }

    public Container GetContainer<T>() where T : class
    {
        var containerName = GetContainerNameForType<T>();
        return GetContainer(containerName);
    }

    public Container GetContainer(string containerName)
    {
        return _containerCache.GetOrAdd(containerName, name =>
        {
            _logger.LogDebug("Creating container reference for '{ContainerName}'", name);
            _connectionStats.IncrementContainerReferences();
            return _database.GetContainer(name);
        });
    }

    public CosmosClient GetClient() => _cosmosClient;

    public Database GetDatabase() => _database;

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Testing Cosmos DB connection...");
            
            var startTime = DateTime.UtcNow;
            await _database.ReadAsync(cancellationToken: cancellationToken);
            var responseTime = DateTime.UtcNow - startTime;

            _connectionStats.UpdateLastConnectionTest(true, responseTime);
            _logger.LogDebug("Cosmos DB connection test successful. Response time: {ResponseTime}ms", responseTime.TotalMilliseconds);
            
            return true;
        }
        catch (Exception ex)
        {
            _connectionStats.UpdateLastConnectionTest(false, TimeSpan.Zero);
            _logger.LogError(ex, "Cosmos DB connection test failed");
            return false;
        }
    }

    public CosmosDbConnectionStats GetConnectionStats() => _connectionStats.Clone();

    private static string GetContainerNameForType<T>() where T : class
    {
        var typeName = typeof(T).Name;
        
        return typeName switch
        {
            "UserDocument" => "Users",
            "PersonaDocument" => "Personas",
            "BuddyDocument" => "Personas", // Stored in same container as personas
            "EmailThreadDocument" => "EmailData",
            "BuddyMemoryDocument" => "BuddyMemory",
            _ => throw new InvalidOperationException($"No container mapping defined for type {typeName}")
        };
    }

    private void CollectStats(object? state)
    {
        try
        {
            // Update connection stats
            _connectionStats.UpdateTimestamp();
            
            // You could add more sophisticated monitoring here
            _logger.LogDebug("Connection stats updated. Active containers: {ContainerCount}", _containerCache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to collect connection stats");
        }
    }

    public void Dispose()
    {
        _statsTimer?.Dispose();
        _cosmosClient?.Dispose();
        _logger.LogInformation("Cosmos DB connection manager disposed");
    }
}

/// <summary>
/// Cosmos DB connection statistics
/// </summary>
public class CosmosDbConnectionStats
{
    private readonly object _lock = new();
    private int _containerReferences = 0;
    private DateTime _lastConnectionTest = DateTime.MinValue;
    private bool _lastConnectionTestSuccess = false;
    private TimeSpan _lastConnectionTestResponseTime = TimeSpan.Zero;
    private DateTime _lastStatsUpdate = DateTime.UtcNow;

    /// <summary>
    /// Number of container references created
    /// </summary>
    public int ContainerReferences
    {
        get { lock (_lock) return _containerReferences; }
    }

    /// <summary>
    /// Last connection test timestamp
    /// </summary>
    public DateTime LastConnectionTest
    {
        get { lock (_lock) return _lastConnectionTest; }
    }

    /// <summary>
    /// Whether the last connection test was successful
    /// </summary>
    public bool LastConnectionTestSuccess
    {
        get { lock (_lock) return _lastConnectionTestSuccess; }
    }

    /// <summary>
    /// Response time of the last connection test
    /// </summary>
    public TimeSpan LastConnectionTestResponseTime
    {
        get { lock (_lock) return _lastConnectionTestResponseTime; }
    }

    /// <summary>
    /// Last statistics update timestamp
    /// </summary>
    public DateTime LastStatsUpdate
    {
        get { lock (_lock) return _lastStatsUpdate; }
    }

    /// <summary>
    /// Increment container references counter
    /// </summary>
    internal void IncrementContainerReferences()
    {
        lock (_lock)
        {
            _containerReferences++;
        }
    }

    /// <summary>
    /// Update last connection test results
    /// </summary>
    /// <param name="success">Whether the test was successful</param>
    /// <param name="responseTime">Response time</param>
    internal void UpdateLastConnectionTest(bool success, TimeSpan responseTime)
    {
        lock (_lock)
        {
            _lastConnectionTest = DateTime.UtcNow;
            _lastConnectionTestSuccess = success;
            _lastConnectionTestResponseTime = responseTime;
        }
    }

    /// <summary>
    /// Update statistics timestamp
    /// </summary>
    internal void UpdateTimestamp()
    {
        lock (_lock)
        {
            _lastStatsUpdate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Create a copy of the current statistics
    /// </summary>
    /// <returns>Copy of statistics</returns>
    internal CosmosDbConnectionStats Clone()
    {
        lock (_lock)
        {
            return new CosmosDbConnectionStats
            {
                _containerReferences = _containerReferences,
                _lastConnectionTest = _lastConnectionTest,
                _lastConnectionTestSuccess = _lastConnectionTestSuccess,
                _lastConnectionTestResponseTime = _lastConnectionTestResponseTime,
                _lastStatsUpdate = _lastStatsUpdate
            };
        }
    }
}

/// <summary>
/// Configuration validator for Cosmos DB options
/// </summary>
public class CosmosDbOptionsValidator
{
    /// <summary>
    /// Validate Cosmos DB configuration
    /// </summary>
    /// <param name="options">Options to validate</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateOptions(CosmosDbOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            errors.Add("ConnectionString is required");
        }
        else if (!IsValidConnectionString(options.ConnectionString))
        {
            errors.Add("ConnectionString format is invalid");
        }

        if (string.IsNullOrWhiteSpace(options.DatabaseName))
        {
            errors.Add("DatabaseName is required");
        }

        if (options.DefaultMaxResults <= 0)
        {
            errors.Add("DefaultMaxResults must be greater than 0");
        }

        if (options.RetryAttempts < 0)
        {
            errors.Add("RetryAttempts must be 0 or greater");
        }

        if (options.RequestTimeoutSeconds <= 0)
        {
            errors.Add("RequestTimeoutSeconds must be greater than 0");
        }

        if (options.MaxConnectionsPerEndpoint <= 0)
        {
            errors.Add("MaxConnectionsPerEndpoint must be greater than 0");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    private static bool IsValidConnectionString(string connectionString)
    {
        try
        {
            // Basic validation - should contain AccountEndpoint and AccountKey
            return connectionString.Contains("AccountEndpoint=") && 
                   (connectionString.Contains("AccountKey=") || connectionString.Contains("ResourceToken="));
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Configuration validation result
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether the configuration is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Get formatted error message
    /// </summary>
    /// <returns>Formatted error message</returns>
    public string GetErrorMessage()
    {
        return string.Join(Environment.NewLine, Errors);
    }
}

/// <summary>
/// Cosmos DB configuration extensions
/// </summary>
public static class CosmosDbConfigurationExtensions
{
    /// <summary>
    /// Add Cosmos DB configuration validation
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCosmosDbConfigurationValidation(this IServiceCollection services)
    {
        services.AddOptions<CosmosDbOptions>()
            .Validate(options =>
            {
                var validation = CosmosDbOptionsValidator.ValidateOptions(options);
                return validation.IsValid;
            }, "Cosmos DB configuration is invalid");

        return services;
    }

    /// <summary>
    /// Add Cosmos DB connection manager
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCosmosDbConnectionManager(this IServiceCollection services)
    {
        services.AddSingleton<ICosmosDbConnectionManager, CosmosDbConnectionManager>();
        return services;
    }
}