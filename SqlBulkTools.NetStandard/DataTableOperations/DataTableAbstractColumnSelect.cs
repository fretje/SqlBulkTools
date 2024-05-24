namespace SqlBulkTools;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// 
/// </remarks>
public abstract class DataTableAbstractColumnSelect<T>(DataTableOperations ext, IEnumerable<T> list, HashSet<string> columns)
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    // ReSharper disable InconsistentNaming
    protected DataTableOperations _ext = ext;
    protected IEnumerable<T> _list = list;
    protected Dictionary<string, string> CustomColumnMappings { get; set; } = [];
    protected HashSet<string> _columns = columns;
    protected DataTable _dt = null;

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member   
}
