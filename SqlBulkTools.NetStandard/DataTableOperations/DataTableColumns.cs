namespace SqlBulkTools;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// 
/// </remarks>
public class DataTableColumns<T>(IEnumerable<T> list, Dictionary<string, Type> propTypes, DataTableOperations ext)
{
    private HashSet<string> Columns { get; set; } = [];
    private readonly IEnumerable<T> _list = list;
    private readonly DataTableOperations _ext = ext;
    private readonly Dictionary<string, int> _ordinalDic = [];
    private readonly List<PropInfo> _propertyInfoList = PropInfoList.From<T>(propTypes);

    /// <summary>
    /// Add each column that you want to include in the DataTable manually. 
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public DataTableSingularColumnSelect<T> AddColumn(Expression<Func<T, object>> columnName)
    {
        var propertyName = BulkOperationsHelper.GetPropertyName(columnName);
        Columns.Add(propertyName);
        return new DataTableSingularColumnSelect<T>(_ext, _list, Columns, _ordinalDic, _propertyInfoList);
    }

    /// <summary>
    /// Adds all properties in model that are either value, string, char[] or byte[] type. 
    /// </summary>
    /// <returns></returns>
    public DataTableAllColumnSelect<T> AddAllColumns()
    {
        Columns = BulkOperationsHelper.GetAllValueTypeAndStringColumns(_propertyInfoList);
        return new DataTableAllColumnSelect<T>(_ext, _list, Columns, _ordinalDic, _propertyInfoList);
    }
}
