namespace SqlBulkTools;

/// <summary>
/// 
/// </summary>
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
public abstract class AbstractColumnSelection<T>(BulkOperations bulk, IEnumerable<T> list, string tableName, HashSet<string> columns, Dictionary<string, string> customColumnMappings, string schema, BulkCopySettings bulkCopySettings, List<PropInfo> propertyInfoList)
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    protected readonly BulkOperations bulk = bulk;
    // ReSharper disable InconsistentNaming
    protected IEnumerable<T> _list = list;
    protected string _tableName = tableName;
    protected string _schema = schema;
    protected Dictionary<string, string> CustomColumnMappings { get; } = customColumnMappings;
    protected HashSet<string> _columns = columns;
    protected bool _disableAllIndexes = false;
    protected BulkCopySettings _bulkCopySettings = bulkCopySettings;
    protected List<PropInfo> _propertyInfoList = propertyInfoList;

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member   

    /// <summary>
    /// A bulk insert will attempt to insert all records. If you have any unique constraints on columns, these must be respected. 
    /// Notes: (1) Only the columns configured (via AddColumn) will be evaluated. (3) Use AddAllColumns to add all columns in table. 
    /// </summary>
    /// <returns></returns>
    public BulkInsert<T> BulkInsert()
    {
        return new BulkInsert<T>(bulk, _list, _tableName, _schema, _columns, CustomColumnMappings, _bulkCopySettings, _propertyInfoList);
    }

    /// <summary>
    /// A bulk insert or update is also known as bulk upsert or merge. All matching rows from the source will be updated.
    /// Any unique rows not found in target but exist in source will be added. Notes: (1) BulkInsertOrUpdate requires at least 
    /// one MatchTargetOn property to be configured. (2) Only the columns configured (via AddColumn) 
    /// will be evaluated. (3) Use AddAllColumns to add all columns in table.
    /// </summary>
    /// <returns></returns>
    public BulkInsertOrUpdate<T> BulkInsertOrUpdate()
    {
        return new BulkInsertOrUpdate<T>(bulk, _list, _tableName, _schema, _columns,
            CustomColumnMappings, _bulkCopySettings, _propertyInfoList);
    }

    /// <summary>
    /// A bulk update will attempt to update any matching records. Notes: (1) BulkUpdate requires at least one MatchTargetOn 
    /// property to be configured. (2) Only the columns configured (via AddColumn) will be evaluated. (3) Use AddAllColumns to add all columns in table.
    /// </summary>
    /// <returns></returns>
    public BulkUpdate<T> BulkUpdate()
    {
        return new BulkUpdate<T>(bulk, _list, _tableName, _schema, _columns, 
            CustomColumnMappings, _bulkCopySettings, _propertyInfoList);
    }

    /// <summary>
    /// A bulk delete will delete records when matched. Consider using a DTO with only the needed information (e.g. PK) Notes: 
    /// (1) BulkUpdate requires at least one MatchTargetOn property to be configured.
    /// </summary>
    /// <returns></returns>
    public BulkDelete<T> BulkDelete()
    {
        return new BulkDelete<T>(bulk, _list, _tableName, _schema, _columns,  
            CustomColumnMappings, _bulkCopySettings, _propertyInfoList);
    }
}
