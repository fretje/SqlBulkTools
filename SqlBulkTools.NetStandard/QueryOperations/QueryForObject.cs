namespace SqlBulkTools.QueryOperations;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// 
/// </remarks>
/// <param name="entity"></param>
/// <param name="sqlParams"></param>
public class QueryForObject<T>(T entity, List<SqlParameter> sqlParams)
{
    private readonly T _entity = entity;
    private readonly List<SqlParameter> _sqlParams = sqlParams;
    private Dictionary<string, Type> _propTypes;

    /// <summary>
    /// Supply the property types when your source is a Dictionary[string, object]
    /// </summary>
    /// <param name="propTypes"></param>
    /// <returns></returns>
    public QueryForObject<T> WithPropertyTypes(Dictionary<string, Type> propTypes)
    {
        _propTypes = propTypes;
        return this;
    }

    /// <summary>
    /// Set the name of table for operation to take place. Registering a table is Required.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <returns></returns>
    public QueryTable<T> WithTable(string tableName)
    {
        var table = BulkOperationsHelper.GetTableAndSchema(tableName);
        return new QueryTable<T>(_entity, _propTypes, table.Name, table.Schema, _sqlParams);
    }
}
