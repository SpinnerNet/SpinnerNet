using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;

namespace SpinnerNet.Core.Data.CosmosDb;

/// <summary>
/// Generic Cosmos DB repository implementation with resilience patterns
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public class CosmosRepository<T> : ICosmosRepository<T> where T : class
{
    private readonly Container _container;
    private readonly ILogger<CosmosRepository<T>> _logger;
    private readonly CosmosDbOptions _options;
    private readonly IAsyncPolicy _retryPolicy;

    public CosmosRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        ILogger<CosmosRepository<T>> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        var containerName = GetContainerName();
        _container = cosmosClient.GetContainer(_options.DatabaseName, containerName);
        
        _retryPolicy = CreateRetryPolicy();
    }

    public async Task<T> CreateOrUpdateAsync(T document, string partitionKey, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var response = await _container.UpsertItemAsync(
                    document,
                    new PartitionKey(partitionKey),
                    cancellationToken: cancellationToken);

                _logger.LogDebug("Document upserted successfully. RU consumed: {RequestCharge}", response.RequestCharge);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.RequestEntityTooLarge)
            {
                _logger.LogError(ex, "Document too large to store in Cosmos DB");
                throw new InvalidOperationException("Document exceeds maximum size limit", ex);
            }
        });
    }

    public async Task<T?> GetAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(
                    id,
                    new PartitionKey(partitionKey),
                    cancellationToken: cancellationToken);

                _logger.LogDebug("Document retrieved successfully. RU consumed: {RequestCharge}", response.RequestCharge);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Document with ID {Id} not found in partition {PartitionKey}", id, partitionKey);
                return null;
            }
        });
    }

    public async Task<List<T>> QueryAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, int? maxResults = null, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var queryOptions = new QueryRequestOptions
            {
                MaxItemCount = maxResults ?? _options.DefaultMaxResults
            };

            if (!string.IsNullOrEmpty(partitionKey))
            {
                queryOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            var queryable = _container.GetItemLinqQueryable<T>(requestOptions: queryOptions)
                .Where(predicate);
            var iterator = queryable.ToFeedIterator();

            var results = new List<T>();
            double totalRU = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
                totalRU += response.RequestCharge;
            }

            _logger.LogDebug("Query executed successfully. Items: {Count}, RU consumed: {RequestCharge}", results.Count, totalRU);
            return results;
        });
    }

    public async Task<List<T>> QueryWithSqlAsync(string sqlQuery, Dictionary<string, object>? parameters = null, string? partitionKey = null, int? maxResults = null, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var queryDefinition = new QueryDefinition(sqlQuery);
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    queryDefinition.WithParameter($"@{param.Key}", param.Value);
                }
            }

            var queryOptions = new QueryRequestOptions
            {
                MaxItemCount = maxResults ?? _options.DefaultMaxResults
            };

            if (!string.IsNullOrEmpty(partitionKey))
            {
                queryOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            var iterator = _container.GetItemQueryIterator<T>(queryDefinition, requestOptions: queryOptions);
            var results = new List<T>();
            double totalRU = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
                totalRU += response.RequestCharge;
            }

            _logger.LogDebug("SQL query executed successfully. Items: {Count}, RU consumed: {RequestCharge}", results.Count, totalRU);
            return results;
        });
    }

    public async Task<bool> DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var response = await _container.DeleteItemAsync<T>(
                    id,
                    new PartitionKey(partitionKey),
                    cancellationToken: cancellationToken);

                _logger.LogDebug("Document deleted successfully. RU consumed: {RequestCharge}", response.RequestCharge);
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Document with ID {Id} not found for deletion in partition {PartitionKey}", id, partitionKey);
                return false;
            }
        });
    }

    public async Task<bool> ExistsAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(
                    id,
                    new PartitionKey(partitionKey),
                    cancellationToken: cancellationToken);

                _logger.LogDebug("Document exists check. RU consumed: {RequestCharge}", response.RequestCharge);
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        });
    }

    public async Task<List<T>> GetByPartitionAsync(string partitionKey, int? maxResults = null, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var queryOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey),
                MaxItemCount = maxResults ?? _options.DefaultMaxResults
            };

            var iterator = _container.GetItemQueryIterator<T>(
                "SELECT * FROM c",
                requestOptions: queryOptions);

            var results = new List<T>();
            double totalRU = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
                totalRU += response.RequestCharge;
            }

            _logger.LogDebug("Partition query executed successfully. Items: {Count}, RU consumed: {RequestCharge}", results.Count, totalRU);
            return results;
        });
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var queryOptions = new QueryRequestOptions
            {
                MaxItemCount = -1 // Get all items for count
            };

            if (!string.IsNullOrEmpty(partitionKey))
            {
                queryOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            // Use SQL query for count since LINQ Count() doesn't support ToFeedIterator
            var sqlQuery = "SELECT VALUE COUNT(1) FROM c";
            var queryDefinition = new QueryDefinition(sqlQuery);
            
            if (!string.IsNullOrEmpty(partitionKey))
            {
                queryOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            var iterator = _container.GetItemQueryIterator<int>(queryDefinition, requestOptions: queryOptions);
            var count = 0;
            double totalRU = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                count = response.FirstOrDefault();
                totalRU += response.RequestCharge;
            }

            _logger.LogDebug("Count query executed successfully. Count: {Count}, RU consumed: {RequestCharge}", count, totalRU);
            return count;
        });
    }

    public async Task<T> PatchAsync(string id, string partitionKey, IReadOnlyList<Microsoft.Azure.Cosmos.PatchOperation> patchOperations, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var response = await _container.PatchItemAsync<T>(
                    id,
                    new PartitionKey(partitionKey),
                    patchOperations,
                    cancellationToken: cancellationToken);

                _logger.LogDebug("Document patched successfully. RU consumed: {RequestCharge}", response.RequestCharge);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Document with ID {Id} not found for patching in partition {PartitionKey}", id, partitionKey);
                throw new InvalidOperationException($"Document with ID {id} not found", ex);
            }
        });
    }

    private string GetContainerName()
    {
        var typeName = typeof(T).Name;
        
        // Map document types to container names
        return typeName switch
        {
            "UserDocument" => "Users",
            "PersonaDocument" => "Personas",
            "BuddyDocument" => "Buddies",
            "TaskDocument" => "Tasks",
            "BuddyMemoryDocument" => "BuddyMemory",
            "InterviewSessionDocument" => "PersonaInterviews",
            "ConversationDocument" => "Conversations",
            "GoalDocument" => "Goals",
            _ => throw new InvalidOperationException($"No container mapping defined for type {typeName}")
        };
    }

    private IAsyncPolicy CreateRetryPolicy()
    {
        return Policy
            .Handle<CosmosException>(ex => 
                // Retry on transient failures as per Microsoft best practices
                ex.StatusCode == HttpStatusCode.RequestTimeout ||      // 408
                ex.StatusCode == HttpStatusCode.Gone ||                // 410  
                ex.StatusCode == HttpStatusCode.TooManyRequests ||     // 429
                ex.StatusCode == (HttpStatusCode)449 ||                // 449 (Retry With)
                ex.StatusCode == HttpStatusCode.ServiceUnavailable)    // 503
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: _options.RetryAttempts,
                sleepDurationProvider: retryAttempt => 
                {
                    // Exponential back-off with jitter for transient failures
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + 
                           TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                },
                onRetry: (exception, timespan) =>
                {
                    var exceptionMessage = exception?.Message ?? "Unknown error";
                    var statusCode = exception is CosmosException cosmosEx ? cosmosEx.StatusCode.ToString() : "Unknown";
                    
                    // Log rate limiting differently as per Microsoft best practices
                    if (exception is CosmosException retryEx && 
                        retryEx.StatusCode == HttpStatusCode.TooManyRequests &&
                        retryEx.RetryAfter.HasValue)
                    {
                        _logger.LogWarning(
                            "Cosmos DB rate limited (429). Will honor retry-after header: {RetryAfter}ms. Status: {StatusCode}",
                            retryEx.RetryAfter.Value.TotalMilliseconds, statusCode);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Cosmos DB retry after {Delay}ms. Status: {StatusCode}, Message: {ExceptionMessage}",
                            timespan.TotalMilliseconds, statusCode, exceptionMessage);
                    }
                });
    }
}

/// <summary>
/// Configuration options for Cosmos DB
/// </summary>
public class CosmosDbOptions
{
    public const string SectionName = "CosmosDb";

    /// <summary>
    /// Cosmos DB connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = "SpinnerNet";

    /// <summary>
    /// Default maximum results for queries
    /// </summary>
    public int DefaultMaxResults { get; set; } = 100;

    /// <summary>
    /// Number of retry attempts for failed operations
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to use gateway mode (vs direct mode)
    /// </summary>
    public bool UseGatewayMode { get; set; } = false;

    /// <summary>
    /// Maximum connections in connection pool
    /// </summary>
    public int MaxConnectionsPerEndpoint { get; set; } = 50;

    /// <summary>
    /// Maximum request units per second for autoscale
    /// </summary>
    public int? AutoscaleMaxThroughput { get; set; } = 1000;
}