namespace SqlBulkTools;

/// <summary>
/// Configurable options for table. 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// 
/// </remarks>
/// <param name="tableName"></param>
public class DeleteQueryTable<T>(string tableName)
{
    private string _schema = Constants.DefaultSchemaName;
    private readonly string _tableName = tableName;
    private int _sqlTimeout = 600;

    /// <summary>
    /// All rows matching the condition(s) selected will be deleted. If you need to delete a collection of objects that can't be
    /// matched by a generic condition, use the BulkDelete method instead. 
    /// </summary>
    /// <returns></returns>
    public DeleteQueryCondition<T> Delete()
    {
        return new DeleteQueryCondition<T>(_tableName, _schema, _sqlTimeout);
    }

    /// <summary>
    /// Explicitly set a schema. If a schema is not added, the system default schema name 'dbo' will used.
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public DeleteQueryTable<T> WithSchema(string schema)
    {
        _schema = schema;
        return this;
    }

    /// <summary>
    /// Default is 600 seconds. See docs for more info. 
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public DeleteQueryTable<T> WithSqlCommandTimeout(int seconds)
    {
        _sqlTimeout = seconds;
        return this;
    }
}
