using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace SpinnerNet.Core.Data.CosmosDb;

/// <summary>
/// Query builder implementation for Cosmos DB
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public class CosmosQueryBuilder<T> : ICosmosQueryBuilder<T> where T : class
{
    private readonly ICosmosRepository<T> _repository;
    private readonly ILogger<CosmosQueryBuilder<T>> _logger;
    private Expression<Func<T, bool>>? _whereClause;
    private (Expression<Func<T, object>> expression, bool descending)? _orderByClause;
    private int? _takeCount;
    private int? _skipCount;
    private string? _partitionKey;

    public CosmosQueryBuilder(ICosmosRepository<T> repository, ILogger<CosmosQueryBuilder<T>> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public ICosmosQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (_whereClause == null)
        {
            _whereClause = predicate;
        }
        else
        {
            // Combine with AND
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                Expression.Invoke(_whereClause, parameter),
                Expression.Invoke(predicate, parameter));
            _whereClause = Expression.Lambda<Func<T, bool>>(body, parameter);
        }
        return this;
    }

    public ICosmosQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> orderBy, bool descending = false)
    {
        // Convert TKey to object for storage
        var objectExpression = Expression.Lambda<Func<T, object>>(
            Expression.Convert(orderBy.Body, typeof(object)),
            orderBy.Parameters);
            
        _orderByClause = (objectExpression, descending);
        return this;
    }

    public ICosmosQueryBuilder<T> Take(int count)
    {
        _takeCount = count;
        return this;
    }

    public ICosmosQueryBuilder<T> Skip(int count)
    {
        _skipCount = count;
        return this;
    }

    public ICosmosQueryBuilder<T> InPartition(string partitionKey)
    {
        _partitionKey = partitionKey;
        return this;
    }

    public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // For complex queries with skip/take, we need to use SQL
            if (_skipCount.HasValue || _orderByClause.HasValue)
            {
                return await ExecuteSqlQueryAsync(cancellationToken);
            }

            // Simple LINQ query
            var predicate = _whereClause ?? (_ => true);
            var results = await _repository.QueryAsync(predicate, _partitionKey, _takeCount, cancellationToken);

            _logger.LogDebug("Query builder executed successfully. Results: {Count}", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute query");
            throw;
        }
    }

    public async Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var originalTake = _takeCount;
        _takeCount = 1;

        try
        {
            var results = await ToListAsync(cancellationToken);
            return results.FirstOrDefault();
        }
        finally
        {
            _takeCount = originalTake;
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var predicate = _whereClause ?? (_ => true);
            var count = await _repository.CountAsync(predicate, _partitionKey, cancellationToken);

            _logger.LogDebug("Count query executed successfully. Count: {Count}", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute count query");
            throw;
        }
    }

    private async Task<List<T>> ExecuteSqlQueryAsync(CancellationToken cancellationToken)
    {
        var sqlBuilder = new CosmosqlQueryBuilder();
        var sql = sqlBuilder.BuildQuery<T>(_whereClause, _orderByClause, _skipCount, _takeCount);

        _logger.LogDebug("Executing SQL query: {SQL}", sql.Query);

        return await _repository.QueryWithSqlAsync(
            sql.Query,
            sql.Parameters,
            _partitionKey,
            _takeCount,
            cancellationToken);
    }
}

/// <summary>
/// SQL query builder for complex Cosmos DB queries
/// </summary>
internal class CosmosqlQueryBuilder
{
    public (string Query, Dictionary<string, object> Parameters) BuildQuery<T>(
        Expression<Func<T, bool>>? whereClause,
        (Expression<Func<T, object>> expression, bool descending)? orderByClause,
        int? skipCount,
        int? takeCount)
    {
        var parameters = new Dictionary<string, object>();
        var sql = "SELECT * FROM c";

        // Add WHERE clause
        if (whereClause != null)
        {
            var whereVisitor = new SqlExpressionVisitor(parameters);
            var whereCondition = whereVisitor.Visit(whereClause.Body);
            sql += $" WHERE {whereCondition}";
        }

        // Add ORDER BY clause
        if (orderByClause.HasValue)
        {
            var orderVisitor = new SqlExpressionVisitor(parameters);
            var orderExpression = orderVisitor.Visit(orderByClause.Value.expression.Body);
            var direction = orderByClause.Value.descending ? "DESC" : "ASC";
            sql += $" ORDER BY {orderExpression} {direction}";
        }

        // Add OFFSET/LIMIT
        if (skipCount.HasValue)
        {
            sql += $" OFFSET {skipCount.Value} LIMIT {takeCount ?? 1000}";
        }
        else if (takeCount.HasValue)
        {
            sql += $" OFFSET 0 LIMIT {takeCount.Value}";
        }

        return (sql, parameters);
    }
}

/// <summary>
/// Expression visitor to convert LINQ expressions to SQL
/// </summary>
internal class SqlExpressionVisitor : ExpressionVisitor
{
    private readonly Dictionary<string, object> _parameters;
    private int _parameterIndex = 0;

    public SqlExpressionVisitor(Dictionary<string, object> parameters)
    {
        _parameters = parameters;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        var left = Visit(node.Left);
        var right = Visit(node.Right);

        var operatorString = node.NodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Binary operator {node.NodeType} is not supported")
        };

        return Expression.Constant($"({left} {operatorString} {right})");
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression?.NodeType == ExpressionType.Parameter)
        {
            // Convert C# property names to JSON property names
            var propertyName = ConvertToJsonPropertyName(node.Member.Name);
            return Expression.Constant($"c.{propertyName}");
        }

        return base.VisitMember(node);
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value == null)
        {
            return Expression.Constant("null");
        }

        var paramName = $"param{_parameterIndex++}";
        _parameters[paramName] = node.Value;
        return Expression.Constant($"@{paramName}");
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Handle string methods like Contains, StartsWith, etc.
        if (node.Method.DeclaringType == typeof(string))
        {
            switch (node.Method.Name)
            {
                case "Contains":
                    var obj = Visit(node.Object!);
                    var arg = Visit(node.Arguments[0]);
                    return Expression.Constant($"CONTAINS({obj}, {arg})");

                case "StartsWith":
                    obj = Visit(node.Object!);
                    arg = Visit(node.Arguments[0]);
                    return Expression.Constant($"STARTSWITH({obj}, {arg})");

                case "EndsWith":
                    obj = Visit(node.Object!);
                    arg = Visit(node.Arguments[0]);
                    return Expression.Constant($"ENDSWITH({obj}, {arg})");
            }
        }

        return base.VisitMethodCall(node);
    }

    private static string ConvertToJsonPropertyName(string propertyName)
    {
        // Convert PascalCase to camelCase for JSON properties
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;

        return char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
    }
}