using Microsoft.Azure.Cosmos;
using System.Linq.Expressions;

namespace SpinnerNet.Core.Data.CosmosDb;

/// <summary>
/// Generic repository interface for Cosmos DB operations
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public interface ICosmosRepository<T> where T : class
{
    /// <summary>
    /// Create or update a document
    /// </summary>
    /// <param name="document">Document to create or update</param>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created or updated document</returns>
    Task<T> CreateOrUpdateAsync(T document, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document or null if not found</returns>
    Task<T?> GetAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query documents with LINQ expression
    /// </summary>
    /// <param name="predicate">Query predicate</param>
    /// <param name="partitionKey">Optional partition key for single-partition queries</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query results</returns>
    Task<List<T>> QueryAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, int? maxResults = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query documents with SQL query string
    /// </summary>
    /// <param name="sqlQuery">SQL query string</param>
    /// <param name="parameters">Query parameters</param>
    /// <param name="partitionKey">Optional partition key for single-partition queries</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query results</returns>
    Task<List<T>> QueryWithSqlAsync(string sqlQuery, Dictionary<string, object>? parameters = null, string? partitionKey = null, int? maxResults = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a document exists
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if document exists</returns>
    Task<bool> ExistsAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all documents in a partition
    /// </summary>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All documents in the partition</returns>
    Task<List<T>> GetByPartitionAsync(string partitionKey, int? maxResults = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count documents matching a predicate
    /// </summary>
    /// <param name="predicate">Query predicate</param>
    /// <param name="partitionKey">Optional partition key for single-partition queries</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of matching documents</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patch a document with partial updates
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="patchOperations">Patch operations to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated document</returns>
    Task<T> PatchAsync(string id, string partitionKey, IReadOnlyList<Microsoft.Azure.Cosmos.PatchOperation> patchOperations, CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of work interface for Cosmos DB operations
/// </summary>
public interface ICosmosUnitOfWork
{
    /// <summary>
    /// Get repository for document type
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    /// <returns>Repository instance</returns>
    ICosmosRepository<T> GetRepository<T>() where T : class;

    /// <summary>
    /// Execute multiple operations in a transaction batch
    /// </summary>
    /// <param name="operations">Operations to execute</param>
    /// <param name="partitionKey">Partition key for all operations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction response</returns>
    Task<TransactionalBatchResponse> ExecuteTransactionAsync(IEnumerable<ICosmosOperation> operations, string partitionKey, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for Cosmos DB operations that can be batched
/// </summary>
public interface ICosmosOperation
{
    /// <summary>
    /// Add operation to TransactionalBatch
    /// </summary>
    /// <param name="batch">The batch to add to</param>
    /// <returns>Updated batch</returns>
    TransactionalBatch AddToBatch(TransactionalBatch batch);
}

/// <summary>
/// Query builder interface for complex queries
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public interface ICosmosQueryBuilder<T> where T : class
{
    /// <summary>
    /// Add WHERE clause
    /// </summary>
    /// <param name="predicate">Predicate expression</param>
    /// <returns>Query builder</returns>
    ICosmosQueryBuilder<T> Where(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Add ORDER BY clause
    /// </summary>
    /// <param name="orderBy">Order by expression</param>
    /// <param name="descending">Order descending</param>
    /// <returns>Query builder</returns>
    ICosmosQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> orderBy, bool descending = false);

    /// <summary>
    /// Set maximum number of results
    /// </summary>
    /// <param name="count">Maximum results</param>
    /// <returns>Query builder</returns>
    ICosmosQueryBuilder<T> Take(int count);

    /// <summary>
    /// Set number of results to skip
    /// </summary>
    /// <param name="count">Number to skip</param>
    /// <returns>Query builder</returns>
    ICosmosQueryBuilder<T> Skip(int count);

    /// <summary>
    /// Set partition key for single-partition query
    /// </summary>
    /// <param name="partitionKey">Partition key value</param>
    /// <returns>Query builder</returns>
    ICosmosQueryBuilder<T> InPartition(string partitionKey);

    /// <summary>
    /// Execute the query
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query results</returns>
    Task<List<T>> ToListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute the query and return first result
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>First result or null</returns>
    Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute the query and count results
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of results</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}