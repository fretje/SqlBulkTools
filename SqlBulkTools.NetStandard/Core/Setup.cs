using SqlBulkTools.QueryOperations;

namespace SqlBulkTools;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="ext"></param>
public class Setup(BulkOperations ext)
{
    private readonly BulkOperations _ext = ext;

    /// <summary>
    /// Represents the collection of objects to be inserted/upserted/updated/deleted (configured in next steps). 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public BulkForCollection<T> ForCollection<T>(IEnumerable<T> list) where T : class
    {
        return new BulkForCollection<T>(_ext, list);
    }

}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// 
/// </remarks>
/// <param name="ext"></param>
public class Setup<T>(BulkOperations ext) where T : class
{
    private readonly BulkOperations _ext = ext;

    private readonly List<SqlParameter> _sqlParams = [];

    ///// <summary>
    ///// Use this option for simple updates or deletes where you are only dealing with a single table 
    ///// and conditions are not complex. For anything more advanced, use a stored procedure.  
    ///// </summary>
    ///// <param name="entity"></param>
    ///// <returns></returns>

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public DeleteQuery<T> ForDeleteQuery()
    {
        return new DeleteQuery<T>();
    }

    /// <summary>
    /// Represents the collection of objects to be inserted/upserted/updated/deleted (configured in next steps). 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public BulkForCollection<T> ForCollection(IEnumerable<T> list)
    {
        return new BulkForCollection<T>(_ext, list);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public QueryForObject<T> ForObject(T entity)
    {
        return new QueryForObject<T>(entity, _sqlParams);
    }       
}
