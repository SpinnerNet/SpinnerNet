using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpinnerNet.Shared.Models.CosmosDb;

namespace SpinnerNet.Core.Data.CosmosDb;

/// <summary>
/// Unit of work implementation for Cosmos DB operations
/// </summary>
public class CosmosUnitOfWork : ICosmosUnitOfWork
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CosmosUnitOfWork> _logger;
    private readonly Dictionary<Type, object> _repositories = new();

    public CosmosUnitOfWork(IServiceProvider serviceProvider, ILogger<CosmosUnitOfWork> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public ICosmosRepository<T> GetRepository<T>() where T : class
    {
        var type = typeof(T);
        
        if (_repositories.TryGetValue(type, out var existingRepository))
        {
            return (ICosmosRepository<T>)existingRepository;
        }

        var repository = _serviceProvider.GetRequiredService<ICosmosRepository<T>>();
        _repositories[type] = repository;
        
        return repository;
    }

    public async Task<TransactionalBatchResponse> ExecuteTransactionAsync(
        IEnumerable<ICosmosOperation> operations, 
        string partitionKey, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the container from the first repository
            // All operations in a transaction must be in the same container and partition
            var firstRepo = _repositories.Values.FirstOrDefault() as CosmosRepository<object>;
            if (firstRepo == null)
            {
                throw new InvalidOperationException("No repositories available for transaction");
            }

            // Create transactional batch
            var cosmosClient = _serviceProvider.GetRequiredService<CosmosClient>();
            var container = GetContainerForTransaction(cosmosClient, operations.First());
            var batch = container.CreateTransactionalBatch(new PartitionKey(partitionKey));

            // Add operations to batch
            foreach (var operation in operations)
            {
                batch = AddOperationToBatch(batch, operation);
            }

            // Execute transaction
            var response = await batch.ExecuteAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Transaction executed successfully with {OperationCount} operations", operations.Count());
            }
            else
            {
                _logger.LogError("Transaction failed with status {StatusCode}: {ErrorMessage}", 
                    response.StatusCode, response.ErrorMessage);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute transaction with {OperationCount} operations", operations.Count());
            throw;
        }
    }

    private Container GetContainerForTransaction(CosmosClient cosmosClient, ICosmosOperation firstOperation)
    {
        // Determine container based on operation type
        var containerName = firstOperation switch
        {
            CreateOperation<UserDocument> => "Users",
            CreateOperation<PersonaDocument> => "Personas", 
            // CreateOperation<BuddyDocument> => "Personas", // COMMENTED OUT FOR SPRINT 1
            CreateOperation<EmailThreadDocument> => "EmailData",
            CreateOperation<BuddyMemoryDocument> => "BuddyMemory",
            _ => throw new InvalidOperationException($"Unknown operation type: {firstOperation.GetType()}")
        };

        return cosmosClient.GetContainer("SpinnerNet", containerName);
    }

    private TransactionalBatch AddOperationToBatch(TransactionalBatch batch, ICosmosOperation operation)
    {
        return operation.AddToBatch(batch);
    }
}

/// <summary>
/// Create operation for transactional batches
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public class CreateOperation<T> : ICosmosOperation where T : class
{
    private readonly T _document;

    public CreateOperation(T document)
    {
        _document = document;
    }

    public TransactionalBatch AddToBatch(TransactionalBatch batch)
    {
        return batch.CreateItem(_document);
    }
}

/// <summary>
/// Update operation for transactional batches
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public class UpdateOperation<T> : ICosmosOperation where T : class
{
    private readonly string _id;
    private readonly T _document;

    public UpdateOperation(string id, T document)
    {
        _id = id;
        _document = document;
    }

    public TransactionalBatch AddToBatch(TransactionalBatch batch)
    {
        return batch.ReplaceItem(_id, _document);
    }
}

/// <summary>
/// Delete operation for transactional batches
/// </summary>
public class DeleteOperation : ICosmosOperation
{
    private readonly string _id;

    public DeleteOperation(string id)
    {
        _id = id;
    }

    public TransactionalBatch AddToBatch(TransactionalBatch batch)
    {
        return batch.DeleteItem(_id);
    }
}

/// <summary>
/// Patch operation for transactional batches
/// </summary>
public class PatchOperation : ICosmosOperation
{
    private readonly string _id;
    private readonly IReadOnlyList<Microsoft.Azure.Cosmos.PatchOperation> _patchOperations;

    public PatchOperation(string id, IReadOnlyList<Microsoft.Azure.Cosmos.PatchOperation> patchOperations)
    {
        _id = id;
        _patchOperations = patchOperations;
    }

    public TransactionalBatch AddToBatch(TransactionalBatch batch)
    {
        return batch.PatchItem(_id, _patchOperations);
    }
}

/// <summary>
/// Extensions for easier repository access from unit of work
/// </summary>
public static class CosmosUnitOfWorkExtensions
{
    /// <summary>
    /// Get user repository
    /// </summary>
    public static ICosmosRepository<UserDocument> Users(this ICosmosUnitOfWork unitOfWork)
        => unitOfWork.GetRepository<UserDocument>();

    /// <summary>
    /// Get persona repository
    /// </summary>
    public static ICosmosRepository<PersonaDocument> Personas(this ICosmosUnitOfWork unitOfWork)
        => unitOfWork.GetRepository<PersonaDocument>();

    /// <summary>
    /// Get buddy repository - COMMENTED OUT FOR SPRINT 1 SIMPLIFICATION
    /// </summary>
    // public static ICosmosRepository<BuddyDocument> Buddies(this ICosmosUnitOfWork unitOfWork)
    //     => unitOfWork.GetRepository<BuddyDocument>();

    /// <summary>
    /// Get email repository
    /// </summary>
    public static ICosmosRepository<EmailThreadDocument> EmailThreads(this ICosmosUnitOfWork unitOfWork)
        => unitOfWork.GetRepository<EmailThreadDocument>();

    /// <summary>
    /// Get buddy memory repository
    /// </summary>
    public static ICosmosRepository<BuddyMemoryDocument> BuddyMemories(this ICosmosUnitOfWork unitOfWork)
        => unitOfWork.GetRepository<BuddyMemoryDocument>();

    /// <summary>
    /// Create a query builder for the specified document type
    /// </summary>
    public static ICosmosQueryBuilder<T> Query<T>(this ICosmosUnitOfWork unitOfWork, IServiceProvider serviceProvider) where T : class
    {
        var repository = unitOfWork.GetRepository<T>();
        var logger = serviceProvider.GetRequiredService<ILogger<CosmosQueryBuilder<T>>>();
        
        return new CosmosQueryBuilder<T>(repository, logger);
    }
}