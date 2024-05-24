using System.Dynamic;

namespace SqlBulkTools;

/// <summary>
/// 
/// </summary>
public class BulkOperations : IBulkOperations
{
    private readonly Dictionary<SchemaKey, DataTable> schemaCache = [];

    /// <summary>
    /// Each transaction requires a valid setup. Examples available at: https://github.com/gtaylor44/SqlBulkTools 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Setup<T> Setup<T>() where T : class
    {
        if (typeof(T) == typeof(ExpandoObject))
        {
            throw new ArgumentException("ExpandoObject is currently not supported.");
        }

        return new Setup<T>(this);
    }

    /// <summary>
    /// Each transaction requires a valid setup. Examples available at: https://github.com/gtaylor44/SqlBulkTools 
    /// </summary>
    /// <returns></returns>
    public Setup Setup()
    {
        return new Setup(this);
    }

    /// <summary>
    /// Utility to prefetch schema information meta data for a given SQL table.
    /// Necessary when using Transaction around Bulk Operations.
    /// </summary>
    public void Prepare(SqlConnection conn, string tableName)
    {
        var table = BulkOperationsHelper.GetTableAndSchema(tableName);
        Prepare(conn, table.Schema, table.Name);
    }
    internal DataTable Prepare(SqlConnection conn, string schema, string tableName)
    {
        var sk = new SchemaKey(conn.Database, schema, tableName);
        if (schemaCache.TryGetValue(sk, out var result))
        {
            return result;
        }

        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }

        var dtCols = conn.GetSchema("Columns", sk.ToRestrictions());

        if (dtCols.Rows.Count == 0 && schema != null)
        {
            throw new SqlBulkToolsException(
                $"Table name '{tableName}' with schema name '{schema}' not found. Check your setup and try again.");
        }

        if (dtCols.Rows.Count == 0)
        {
            throw new SqlBulkToolsException($"Table name '{tableName}' not found. Check your setup and try again.");
        }

        schemaCache[sk] = dtCols;
        return dtCols;
    }

    class SchemaKey(string database, string schema, string tableName)
    {
        public readonly string Database = database, Schema = schema, TableName = tableName;

        public string[] ToRestrictions() =>
        [
            Database,
            Schema,
            TableName,
            null
        ];

        public override int GetHashCode()
        {
            return Database.GetHashCode() ^ Schema.GetHashCode() ^ TableName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is SchemaKey sk
                && sk.Database == Database
                && sk.Schema == Schema
                && sk.TableName == TableName;
        }
    }
}
