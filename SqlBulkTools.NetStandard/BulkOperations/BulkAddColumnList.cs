namespace SqlBulkTools;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// 
/// </remarks>
/// <param name="bulk"></param>
/// <param name="list"></param>
/// <param name="tableName"></param>
/// <param name="columns"></param>
/// <param name="customColumnMappings"></param>
/// <param name="schema"></param>
/// <param name="bulkCopySettings"></param>
/// <param name="propertyInfoList"></param>
public class BulkAddColumnList<T>(BulkOperations bulk, IEnumerable<T> list, string tableName, HashSet<string> columns, Dictionary<string, string> customColumnMappings,
    string schema, BulkCopySettings bulkCopySettings, List<PropInfo> propertyInfoList) 
    : AbstractColumnSelection<T>(bulk, list, tableName, columns, customColumnMappings, schema, bulkCopySettings, propertyInfoList)
{

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
    public BulkAddColumnList<T> CustomColumnMapping(string source, string destination)
    {
        CustomColumnMappings.Add(source, destination);
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
    public BulkAddColumnList<T> CustomColumnMapping(Expression<Func<T, object>> source, string destination)
    {
        var propertyName = BulkOperationsHelper.GetPropertyName(source);
        return CustomColumnMapping(propertyName, destination);
    }

    /// <summary>
    /// Removes a column that you want to be excluded. 
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="SqlBulkToolsException"></exception>
    public BulkAddColumnList<T> RemoveColumn(string columnName)
    {
        if (!_columns.Remove(columnName))
        {
            throw new SqlBulkToolsException("Could not remove the column with name "
                + columnName +
                ". This could be because it's not a value or string type and therefore not included.");
        }

        return this;
    }

    /// <summary>
    /// Removes a column that you want to be excluded. 
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="SqlBulkToolsException"></exception>
    public BulkAddColumnList<T> RemoveColumn(Expression<Func<T, object>> columnName)
    {
        return RemoveColumn(BulkOperationsHelper.GetPropertyName(columnName));
    }
}
