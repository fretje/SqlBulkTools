namespace SqlBulkTools;

/// <summary>
///
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
///
/// </remarks>
/// <param name="tableName"></param>
/// <param name="schema"></param>
/// <param name="conditionSortOrder"></param>
/// <param name="whereConditions"></param>
/// <param name="parameters"></param>
/// <param name="collationColumnDic"></param>
/// <param name="customColumnMappings"></param>
public class DeleteQueryReady<T>(string tableName, string schema, int conditionSortOrder, List<PredicateCondition> whereConditions,
    List<SqlParameter> parameters, Dictionary<string, string> collationColumnDic, Dictionary<string, string> customColumnMappings) : ITransaction
{
    private readonly string _tableName = tableName;
    private readonly string _schema = schema;
    private readonly List<PredicateCondition> _whereConditions = whereConditions;
    private readonly List<PredicateCondition> _andConditions = [];
    private readonly List<PredicateCondition> _orConditions = [];
    private readonly List<SqlParameter> _parameters = parameters;
    private int _conditionSortOrder = conditionSortOrder;
    private readonly Dictionary<string, string> _collationColumnDic = collationColumnDic;
    private readonly Dictionary<string, string> _customColumnMappings = customColumnMappings;
    private int? _batchQuantity = null;

    /// <summary>
    /// Specify an additional condition to match on.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    /// <exception cref="SqlBulkToolsException"></exception>
    public DeleteQueryReady<T> And(Expression<Func<T, bool>> expression)
    {
        BulkOperationsHelper.AddPredicate(expression, PredicateType.And, _andConditions, _parameters,
            _conditionSortOrder, appendParam: Constants.UniqueParamIdentifier);
        _conditionSortOrder++;
        return this;
    }

    /// <summary>
    /// Specify an additional condition to match on.
    /// </summary>
    /// <param name="expression">Only explicitly set the collation if there is a collation conflict.</param>
    /// <param name="collation"></param>
    /// <returns></returns>
    /// <exception cref="SqlBulkToolsException">Only explicitly set the collation if there is a collation conflict.</exception>
    public DeleteQueryReady<T> And(Expression<Func<T, bool>> expression, string collation)
    {
        BulkOperationsHelper.AddPredicate(expression, PredicateType.And, _andConditions, _parameters, _conditionSortOrder, appendParam: Constants.UniqueParamIdentifier);
        _conditionSortOrder++;

        string leftName = BulkOperationsHelper.GetExpressionLeftName(expression, PredicateType.And, "Collation");
        _collationColumnDic.Add(BulkOperationsHelper.GetActualColumn(_customColumnMappings, leftName), collation);

        return this;
    }

    /// <summary>
    /// Specify an additional condition to match on.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    /// <exception cref="SqlBulkToolsException"></exception>
    public DeleteQueryReady<T> Or(Expression<Func<T, bool>> expression)
    {
        BulkOperationsHelper.AddPredicate(expression, PredicateType.Or, _orConditions, _parameters,
            _conditionSortOrder, appendParam: Constants.UniqueParamIdentifier);
        _conditionSortOrder++;
        return this;
    }

    /// <summary>
    /// Specify an additional condition to match on.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="collation">Only explicitly set the collation if there is a collation conflict.</param>
    /// <returns></returns>
    /// <exception cref="SqlBulkToolsException"></exception>
    public DeleteQueryReady<T> Or(Expression<Func<T, bool>> expression, string collation)
    {
        BulkOperationsHelper.AddPredicate(expression, PredicateType.Or, _orConditions, _parameters, _conditionSortOrder, appendParam: Constants.UniqueParamIdentifier);
        _conditionSortOrder++;

        string leftName = BulkOperationsHelper.GetExpressionLeftName(expression, PredicateType.Or, "Collation");
        _collationColumnDic.Add(BulkOperationsHelper.GetActualColumn(_customColumnMappings, leftName), collation);

        return this;
    }

    /// <summary>
    /// The maximum number of records to delete per transaction.
    /// </summary>
    /// <param name="batchQuantity"></param>
    /// <returns></returns>
    public DeleteQueryReady<T> SetBatchQuantity(int batchQuantity)
    {
        _batchQuantity = batchQuantity;
        return this;
    }

    /// <summary>
    /// Commits a transaction to database. A valid setup must exist for the operation to be
    /// successful.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public int Commit(IDbConnection connection, IDbTransaction transaction = null)
    {
        if (connection is SqlConnection == false)
        {
            throw new ArgumentException("Parameter must be a SqlConnection instance");
        }

        return Commit((SqlConnection)connection, (SqlTransaction)transaction);
    }

    /// <summary>
    /// Commits a transaction to database. A valid setup must exist for the operation to be
    /// successful.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Task<int> CommitAsync(IDbConnection connection, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
    {
        if (connection is SqlConnection == false)
        {
            throw new ArgumentException("Parameter must be a SqlConnection instance");
        }

        return CommitAsync((SqlConnection)connection, (SqlTransaction)transaction, cancellationToken);
    }

    /// <summary>
    /// Commits a transaction to database. A valid setup must exist for the operation to be
    /// successful.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public int Commit(SqlConnection connection, SqlTransaction transaction)
    {
        if (connection.State == ConnectionState.Closed)
        {
            connection.Open();
        }

        SqlCommand command = connection.CreateCommand();
        command.Connection = connection;
        command.Transaction = transaction;

        command.CommandText = GetQuery(connection);

        if (_parameters.Count > 0)
        {
            command.Parameters.AddRange([.. _parameters]);
        }

        int affectedRows = command.ExecuteNonQuery();

        return affectedRows;
    }

    /// <summary>
    /// Commits a transaction to database asynchronously. A valid setup must exist for the operation to be
    /// successful.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> CommitAsync(SqlConnection connection, SqlTransaction transaction, CancellationToken cancellationToken)
    {
        if (connection.State == ConnectionState.Closed)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        SqlCommand command = connection.CreateCommand();
        command.Connection = connection;
        command.Transaction = transaction;

        command.CommandText = command.CommandText = GetQuery(connection);

        if (_parameters.Count > 0)
        {
            command.Parameters.AddRange([.. _parameters]);
        }

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        return affectedRows;
    }

    private string GetQuery(SqlConnection connection)
    {
        var concatenatedQuery = _whereConditions.Concat(_andConditions).Concat(_orConditions).OrderBy(x => x.SortOrder);

        string fullQualifiedTableName = BulkOperationsHelper.GetFullQualifyingTableName(connection.Database, _schema,
             _tableName);

        string batchQtyStart = _batchQuantity != null ? "DeleteMore:\n" : string.Empty;
        string batchQty = _batchQuantity != null ? $"TOP ({_batchQuantity}) " : string.Empty;
        string batchQtyRepeat = _batchQuantity != null ? $"\nIF @@ROWCOUNT != 0\ngoto DeleteMore" : string.Empty;

        return $"{batchQtyStart}DELETE {batchQty}FROM {fullQualifiedTableName} " +
               $"{BulkOperationsHelper.BuildPredicateQuery(concatenatedQuery, _collationColumnDic, _customColumnMappings)}{batchQtyRepeat}";
    }
}
