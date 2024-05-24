namespace SqlBulkTools;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// 
/// </remarks>
/// <param name="ext"></param>
/// <param name="list"></param>
/// <param name="columns"></param>
/// <param name="ordinalDic"></param>
/// <param name="propertyInfoList"></param>
public class DataTableAllColumnSelect<T>(DataTableOperations ext, IEnumerable<T> list, HashSet<string> columns, Dictionary<string, int> ordinalDic, List<PropInfo> propertyInfoList) : DataTableAbstractColumnSelect<T>(ext, list, columns), IDataTableTransaction
{
    private readonly HashSet<string> _removedColumns = [];
    private readonly Dictionary<string, int> _ordinalDic = ordinalDic;
    private readonly List<PropInfo> _propertyInfoList = propertyInfoList;

    /// <summary>
    /// If a column name in your model does not match the designated column name in the actual SQL table, 
    /// you can add a custom column mapping.   
    /// </summary>
    /// <returns></returns>
    public DataTableAllColumnSelect<T> CustomColumnMapping(Expression<Func<T, object>> source, string destination)
    {
        var propertyName = BulkOperationsHelper.GetPropertyName(source);
        CustomColumnMappings.Add(propertyName, destination);
        return this;
    }

    /// <summary>
    /// Removes a column that you want to be excluded from the data table. 
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="SqlBulkToolsException"></exception>
    public DataTableAllColumnSelect<T> RemoveColumn(Expression<Func<T, object>> columnName)
    {
        var propertyName = BulkOperationsHelper.GetPropertyName(columnName);
        if (_columns.Contains(propertyName))
        {
            _removedColumns.Add(propertyName);
            _columns.Remove(propertyName);
        }
            

        else
        {
            throw new SqlBulkToolsException("Could not remove the column with name " 
                + columnName +  
                ". This could be because it's not a value or string type and therefore not included.");
        }

        return this;
    }

    /// <summary>
    /// Returns a data table to be used in a stored procedure. 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public DataTable PrepareDataTable()
    {
        _ext.SetBulkExt(this, _columns, CustomColumnMappings, typeof(T), _removedColumns);
        _dt = BulkOperationsHelper.CreateDataTable<T>(_propertyInfoList, _columns, CustomColumnMappings, _ordinalDic);
        return _dt;
    }

    DataTable IDataTableTransaction.BuildDataTable()
    {
        return BulkOperationsHelper.ConvertListToDataTable(_propertyInfoList, _dt, _list, _columns, _ordinalDic);
    }

}
