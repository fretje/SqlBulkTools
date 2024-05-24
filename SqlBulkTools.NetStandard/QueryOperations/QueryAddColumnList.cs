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
/// <param name="columns"></param>
/// <param name="schema"></param>
/// <param name="sqlParams"></param>
/// <param name="propertyInfoList"></param>
public class QueryAddColumnList<T>(T singleEntity, string tableName, HashSet<string> columns, string schema,
    List<SqlParameter> sqlParams, List<PropInfo> propertyInfoList)
{
    private readonly T _singleEntity = singleEntity;
    private readonly string _tableName = tableName;
    private Dictionary<string, string> CustomColumnMappings { get; } = [];
    private readonly HashSet<string> _columns = columns;
    private readonly string _schema = schema;
    private readonly List<SqlParameter> _sqlParams = sqlParams;
    private readonly List<PropInfo> _propertyInfoList = propertyInfoList;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public QueryInsertReady<T> Insert()
    {
        return new QueryInsertReady<T>(_singleEntity, _tableName, _schema, _columns, CustomColumnMappings,
            _sqlParams, _propertyInfoList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public QueryUpsertReady<T> Upsert()
    {
        return new QueryUpsertReady<T>(_singleEntity, _tableName, _schema, _columns, CustomColumnMappings,
             _sqlParams, _propertyInfoList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public QueryUpdateCondition<T> Update()
    {
        return new QueryUpdateCondition<T>(_singleEntity, _tableName, _schema, _columns, CustomColumnMappings, 
             _sqlParams, _propertyInfoList);
    }  

    /// <summary>
    /// Removes a column that you want to be excluded. 
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="SqlBulkToolsException"></exception>
    public QueryAddColumnList<T> RemoveColumn(Expression<Func<T, object>> columnName)
    {
        var propertyName = BulkOperationsHelper.GetPropertyName(columnName);
        if (!_columns.Remove(propertyName))
        {
            throw new SqlBulkToolsException("Could not remove the column with name "
                + columnName +
                ". This could be because it's not a value or string type and therefore not included.");
        }

        return this;
    }

    /// <summary>
    /// By default SqlBulkTools will attempt to match the model property names to SQL column names (case insensitive). 
    /// If any of your model property names do not match 
    /// the SQL table column(s) as defined in given table, then use this method to set up a custom mapping.  
    /// </summary>
    /// <param name="source">
    /// The object member that has a different name in SQL table. 
    /// </param>
    /// <param name="destination">
    /// The actual name of column as represented in SQL table. 
    /// </param>
    /// <returns></returns>
    public QueryAddColumnList<T> CustomColumnMapping(Expression<Func<T, object>> source, string destination)
    {
        var propertyName = BulkOperationsHelper.GetPropertyName(source);
        CustomColumnMappings.Add(propertyName, destination);
        return this;
    }
}
