using SqlBulkTools.BulkCopy;

namespace SqlBulkTools
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="bulk"></param>
    /// <param name="list"></param>
    public class BulkForCollection<T>(BulkOperations bulk, IEnumerable<T> list)
    {
        private readonly BulkOperations bulk = bulk;
        private readonly IEnumerable<T> _list = list;
        private Dictionary<string, Type> _propTypes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propTypes"></param>
        /// <returns></returns>
        public BulkForCollection<T> WithPropertyTypes(Dictionary<string, Type> propTypes)
        {
            _propTypes = propTypes;
            return this;
        }

        /// <summary>
        /// Set the name of table for operation to take place. Registering a table is Required.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public BulkTable<T> WithTable(string tableName)
        {
            var table = BulkOperationsHelper.GetTableAndSchema(tableName);
            return new BulkTable<T>(bulk, _list, _propTypes, table.Name, table.Schema);
        }
    }
}
