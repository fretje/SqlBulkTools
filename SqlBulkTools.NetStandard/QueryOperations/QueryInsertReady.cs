using System.Reflection;
using System.Text;

namespace SqlBulkTools.QueryOperations;

/// <summary>
///
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
///
/// </remarks>
/// <param name="singleEntity"></param>
/// <param name="tableName"></param>
/// <param name="schema"></param>
/// <param name="columns"></param>
/// <param name="customColumnMappings"></param>
/// <param name="sqlParams"></param>
/// <param name="propertyInfoList"></param>
public class QueryInsertReady<T>(T singleEntity, string tableName, string schema, HashSet<string> columns, Dictionary<string, string> customColumnMappings,
    List<SqlParameter> sqlParams, List<PropInfo> propertyInfoList) : ITransaction
{
    private readonly T _singleEntity = singleEntity;
    private readonly string _tableName = tableName;
    private readonly string _schema = schema;
    private readonly HashSet<string> _columns = columns;
    private readonly Dictionary<string, string> _customColumnMappings = customColumnMappings;
    private string _identityColumn;
    private ColumnDirectionType _outputIdentity = ColumnDirectionType.Input;
    private readonly List<SqlParameter> _sqlParams = sqlParams;
    private readonly List<PropInfo> _propertyInfoList = propertyInfoList;

    /// <summary>
    /// Sets the identity column for the table.
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public QueryInsertReady<T> SetIdentityColumn(Expression<Func<T, object>> columnName)
    {
        var propertyName = BulkOperationsHelper.GetPropertyName(columnName) ?? throw new SqlBulkToolsException("SetIdentityColumn column name can't be null");
        if (_identityColumn == null)
        {
            _identityColumn = BulkOperationsHelper.GetActualColumn(_customColumnMappings, propertyName);
        }
        else
        {
            throw new SqlBulkToolsException("Can't have more than one identity column");
        }

        _columns.Add(propertyName);

        return this;
    }

    /// <summary>
    /// Sets the identity column for the table.
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public QueryInsertReady<T> SetIdentityColumn(Expression<Func<T, object>> columnName, ColumnDirectionType direction)
    {
        var propertyName = BulkOperationsHelper.GetPropertyName(columnName);
        _outputIdentity = direction;

        if (propertyName == null)
        {
            throw new SqlBulkToolsException("SetIdentityColumn column name can't be null");
        }

        if (_identityColumn == null)
        {
            if (_customColumnMappings.TryGetValue(propertyName, out string actualPropertyName))
            {
                _identityColumn = actualPropertyName;
            }
            else
            {
                _identityColumn = propertyName;
            }
        }
        else
        {
            throw new SqlBulkToolsException("Can't have more than one identity column");
        }

        _columns.Add(propertyName);

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
    /// <exception cref="IdentityException"></exception>
    public int Commit(SqlConnection connection, SqlTransaction transaction)
    {
        int affectedRows = 0;
        if (_singleEntity == null)
        {
            return affectedRows;
        }

        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        try
        {
            BulkOperationsHelper.AddSqlParamsForQuery(_propertyInfoList, _sqlParams, _columns, _singleEntity, _identityColumn, _outputIdentity, _customColumnMappings);
            BulkOperationsHelper.DoColumnMappings(_customColumnMappings, _columns);

            SqlCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = transaction;

            string fullQualifiedTableName = BulkOperationsHelper.GetFullQualifyingTableName(connection.Database, _schema,
            _tableName);

            var sb = new StringBuilder();

            sb.Append($"{BulkOperationsHelper.BuildInsertIntoSet(_columns, _identityColumn, fullQualifiedTableName)} " +
                          $"VALUES{BulkOperationsHelper.BuildValueSet(_columns, _identityColumn)} ");

            if (_outputIdentity == ColumnDirectionType.InputOutput)
            {
                sb.Append($"SET @{_identityColumn}=SCOPE_IDENTITY();");
            }

            command.CommandText = sb.ToString();

            if (_sqlParams.Count > 0)
            {
                command.Parameters.AddRange([.. _sqlParams]);
            }

            affectedRows = command.ExecuteNonQuery();

            if (_outputIdentity == ColumnDirectionType.InputOutput)
            {
                foreach (var x in _sqlParams)
                {
                    if (x.Direction == ParameterDirection.InputOutput
                        && x.ParameterName == $"@{_identityColumn}")
                    {
                        PropertyInfo propertyInfo = _singleEntity.GetType().GetProperty(_identityColumn);
                        propertyInfo.SetValue(_singleEntity, x.Value);
                        break;
                    }
                }
            }

            return affectedRows;
        }
        catch (SqlException e)
        {
            for (int i = 0; i < e.Errors.Count; i++)
            {
                // Error 8102 and 544 is identity error.
                if (e.Errors[i].Number == 544 || e.Errors[i].Number == 8102)
                {
                    // Expensive but neccessary to inform user of an important configuration setup.
                    throw new IdentityException(e.Errors[i].Message);
                }
            }
            throw;
        }
    }

    /// <summary>
    /// Commits a transaction to database asynchronously. A valid setup must exist for the operation to be
    /// successful.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="IdentityException"></exception>
    public async Task<int> CommitAsync(SqlConnection connection, SqlTransaction transaction, CancellationToken cancellationToken)
    {
        int affectedRows = 0;
        if (_singleEntity == null)
        {
            return affectedRows;
        }

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            BulkOperationsHelper.AddSqlParamsForQuery(_propertyInfoList, _sqlParams, _columns, _singleEntity, _identityColumn, _outputIdentity, _customColumnMappings);
            BulkOperationsHelper.DoColumnMappings(_customColumnMappings, _columns);

            SqlCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.Transaction = transaction;

            string fullQualifiedTableName = BulkOperationsHelper.GetFullQualifyingTableName(connection.Database, _schema,
            _tableName);

            var sb = new StringBuilder();

            sb.Append($"{BulkOperationsHelper.BuildInsertIntoSet(_columns, _identityColumn, fullQualifiedTableName)} " +
                      $"VALUES{BulkOperationsHelper.BuildValueSet(_columns, _identityColumn)} ");

            if (_outputIdentity == ColumnDirectionType.InputOutput)
            {
                sb.Append($"SET @{_identityColumn}=SCOPE_IDENTITY();");
            }

            command.CommandText = sb.ToString();

            if (_sqlParams.Count > 0)
            {
                command.Parameters.AddRange([.. _sqlParams]);
            }

            affectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            if (_outputIdentity == ColumnDirectionType.InputOutput)
            {
                foreach (var x in _sqlParams)
                {
                    if (x.Direction == ParameterDirection.InputOutput
                        && x.ParameterName == $"@{_identityColumn}")
                    {
                        PropertyInfo propertyInfo = _singleEntity.GetType().GetProperty(_identityColumn);
                        propertyInfo.SetValue(_singleEntity, x.Value);
                        break;
                    }
                }
            }

            return affectedRows;
        }
        catch (SqlException e)
        {
            for (int i = 0; i < e.Errors.Count; i++)
            {
                // Error 8102 and 544 is identity error.
                if (e.Errors[i].Number == 544 || e.Errors[i].Number == 8102)
                {
                    // Expensive but neccessary to inform user of an important configuration setup.
                    throw new IdentityException(e.Errors[i].Message);
                }
            }
            throw;
        }
    }
}
