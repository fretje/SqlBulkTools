namespace SqlBulkTools.QueryOperations;

/// <summary>
/// Configurable options for table. 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// 
/// </remarks>
/// <param name="singleEntity"></param>
/// <param name="propTypes"></param>
/// <param name="tableName"></param>
/// <param name="schema"></param>
/// <param name="sqlParams"></param>
public class QueryTable<T>(T singleEntity, Dictionary<string, Type> propTypes, string tableName, string schema, List<SqlParameter> sqlParams)
{
    private readonly T _singleEntity = singleEntity;
    private HashSet<string> Columns { get; set; } = [];
    private string _schema = schema;
    private readonly string _tableName = tableName;
    private readonly List<SqlParameter> _sqlParams = sqlParams;
    private readonly List<PropInfo> _propertyInfoList = PropInfoList.From<T>(propTypes);

    /// <summary>
    /// Add each column that you want to include in the query.
    /// </summary>
    /// <param name="columnName">Column name as represented in database</param>
    /// <returns></returns>
    public QueryAddColumn<T> AddColumn(Expression<Func<T, object>> columnName)
    {
        var propertyName = BulkOperationsHelper.GetPropertyName(columnName);
        Columns.Add(propertyName);
        return new QueryAddColumn<T>(_singleEntity, _tableName, Columns, _schema, _sqlParams, _propertyInfoList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public QueryAddColumnList<T> AddAllColumns()
    {
        Columns = BulkOperationsHelper.GetAllValueTypeAndStringColumns(_propertyInfoList);

        return new QueryAddColumnList<T>(_singleEntity, _tableName, Columns, _schema, _sqlParams, _propertyInfoList);
    }

    /// <summary>
    /// Explicitly set a schema. If a schema is not added, the system default schema name 'dbo' will used.
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public QueryTable<T> WithSchema(string schema)
    {
        if (_schema != Constants.DefaultSchemaName)
        {
            throw new SqlBulkToolsException("Schema has already been defined in WithTable method.");
        }

        _schema = schema;
        return this;
    }
}
