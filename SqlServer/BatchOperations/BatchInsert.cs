using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Buffers;
using Utils;

namespace SqlServer.BatchOperations
{

    public enum ExecuteMode
    {
        /// <summary>
        /// Sql is Executing in HDD or an driver with less 200Mb/s or low IOPS
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Sql is executing in an driver with at least 400MB/s(highly recommended use this mode only when BD is in cold state)
        /// </summary>
        HighPerformance = 2
    }

    public class BatchInsert<T> where T : class
    {
        public readonly string TableName;
        public readonly string TempTableName;
        private readonly Dictionary<string, PropertyInfo> Properties;
        private Common.Logging.ILog LOG;
        private string OriginalConnectionString;
        private int BucketCount;
        private ExecuteMode ExecuteMode;
        private SqlConnection TempTableKeeper;
        private Task TempTableKeepAlive;
        private CancellationTokenSource CancellationToken;

        public BatchInsert(string connectionString, Common.Logging.ILog log, ExecuteMode executeMode)
        {
            try
            {
                OriginalConnectionString = connectionString;
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                builder.ApplicationName = "BatchInsert";
                builder.Pooling = false;
                builder.LoadBalanceTimeout = 0;

                LOG = log;

                ExecuteMode = executeMode;
                TableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name
                        ?? throw new InvalidOperationException("You can only use sources that are marked TableAttribute.");
                TempTableName = "##temp_" + TableName + DateTime.Now.Millisecond;

                LOG.Info("Temp table name " + TempTableName);

                SqlConnection con = new SqlConnection(builder.ToString());
                con.Open();
                CheckIfProceduresExists(con);
                CreateTempTable(con);
                Properties = new Dictionary<string, PropertyInfo>();
                DiscoveryPropertiesSchemaOrder(con);
                TempTableKeeper = con;
                Console.WriteLine("Temp table name " + TempTableName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                log.Error(ex.Message);
                throw ex;
            }
        }

        private void CheckIfProceduresExists(SqlConnection con)
        {
            using (SqlCommand command = new SqlCommand())
            {
                command.CommandType = CommandType.Text;
                command.Connection = con;

                command.CommandText = "sp_helptext sp_CreateTempTable";
                _ = command.ExecuteNonQuery();

                command.CommandText = "sp_helptext sp_UpSert";
                _ = command.ExecuteNonQuery();

                command.CommandText = "sp_helptext sp_UpdateFromTempTable";
                _ = command.ExecuteNonQuery();

                command.CommandText = "sp_helptext sp_InsertFromTemp";
                _ = command.ExecuteNonQuery();
            }
        }

        private void CreateTempTable(SqlConnection con)
        {
            using (SqlCommand commandCreateTemp = new SqlCommand("dbo.sp_CreateTempTable", con))
            {
                commandCreateTemp.CommandType = CommandType.StoredProcedure;
                commandCreateTemp.Parameters.Add(new SqlParameter("@tempTableName", SqlDbType.VarChar)).Value = TempTableName.Replace("##", "");
                commandCreateTemp.Parameters.Add(new SqlParameter("@originTable", SqlDbType.VarChar)).Value = TableName;
                commandCreateTemp.ExecuteNonQuery();
            }
        }

        private void DiscoveryPropertiesSchemaOrder(SqlConnection con)
        {
            List<Tuple<PropertyInfo, ColumnAttribute>> properties = typeof(T).GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                                       .Where(p => p.IsDefined(typeof(ColumnAttribute)) && !p.IsDefined(typeof(NotMappedAttribute)))
                                       .Select(p => Tuple.Create(p, (ColumnAttribute)p.GetCustomAttribute(typeof(ColumnAttribute))))
                                       .ToList();

            Tuple<PropertyInfo, ColumnAttribute> currentProperty;
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select [name] from sys.columns where object_id = OBJECT_ID('" + TableName + "') order by column_id";
                com.CommandType = CommandType.Text;
                var ordenedColumns = com.ExecuteReader();

                if (ordenedColumns.HasRows)
                {
                    while (ordenedColumns.Read())
                    {
                        string currentName = ordenedColumns[0].ToString();
                        currentProperty = properties.FirstOrDefault(p => p.Item2.Name == currentName);
                        if (currentProperty == null)
                        {
                            currentProperty = properties.FirstOrDefault(p => p.Item1.Name == currentName);
                            if (currentProperty == null)
                                continue;
                        }
                        Properties.Add(currentName, currentProperty.Item1);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Table doesn't exists, or unhandled error occurred when trying to get DataBase Schema");
                }
            }
        }

        public void AddToTempTable(IEnumerable<T> source)
        {
            var type = typeof(T).Name;
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(TempTableKeeper))
            {
                bulkCopy.BatchSize = 0;//Helps the operation stay minimal logged
                bulkCopy.DestinationTableName = TempTableName;
                bulkCopy.BulkCopyTimeout = 750;//Required when the database is overloaded

                foreach (List<T> bucket in source.Partition(200000))//This specific amount helps the operation stay minimal logged
                {
                    try
                    {
                        string message = "Writing "+ bucket.Count +" objects of type " + type + " to temp_table " + TempTableName;
                        LOG.Trace(message);
                        bulkCopy.WriteToServer(bucket.ToDataTable());

                        BucketCount++;
                        LOG.Info("Sucessful added items to database");
                    }
                    catch (Exception ex)
                    {
                        LOG.Error($"Error while writing to database TABLE:{bulkCopy.DestinationTableName}, type: {typeof(T)}");
                        LOG.Error(ex.Message);
                    }
                }
            }
        }

        public void AddToTempTable(Span<T> source)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(TempTableKeeper))
            {
                bulkCopy.BatchSize = 0;//Ajuda a operação ficar minimal logged
                bulkCopy.DestinationTableName = TempTableName;
                bulkCopy.BulkCopyTimeout = 750;//Necessário para quando o banco de dados está sobrecarregado

                const int sliceBy = 1;
                int taken = 0;
                int left = source.Length;
                DataTable bucketAsDataTable = new DataTable();
                while (left > 0)
                {
                    try
                    {
                        ReadOnlySpan<T> bucket = source.Slice(taken, sliceBy > left ? left : sliceBy);
                        taken += sliceBy;
                        left -= sliceBy;

                        LOG.Trace($"Writing {bucket.Length} objects of type {typeof(T)} to temp_table {TempTableName}");
                        bucketAsDataTable = bucket.ToDataTable(Properties);
                        bulkCopy.WriteToServer(bucketAsDataTable);

                        BucketCount++;
                        LOG.Info("Sucessful added items to database");
                    }
                    catch (Exception ex)
                    {
                        LOG.Error($"Error while writing to database TABLE:{bulkCopy.DestinationTableName}, type: {typeof(T)}");
                        LOG.Error(ex.Message);
                    }
                }
            }
        }

        public void FinalizeInsertions(bool insert, bool update)
        {
            if (insert && update)
            {
                if (ExecuteMode == ExecuteMode.HighPerformance)
                {
                    List<Task> tasks = new List<Task>(2);

                    tasks.Add(Task.Factory.StartNew(() => PerformInsert()));
                    tasks.Add(Task.Factory.StartNew(() => PerformUpdate()));

                    Task.WaitAll(tasks.ToArray());
                }
                else
                {
                    PerformOperations();
                }
            }
            else if (insert)
            {
                PerformInsert();
            }
            else if (update)
            {
                PerformUpdate();
            }
        }

        /// <summary>
        /// As procedures são mais rápidas que utilizar o MERGE do SQL
        /// Talvez vale testar novamente, caso a Microsoft faça upgrades no Merge
        /// Data dos teste 15/12/2019
        /// </summary>
        /// <param name="source"></param>
        private void PerformOperations()
        {
            PerformUpdate();
            PerformInsert();
        }

        private void PerformInsert()
        {
            using (var con = new SqlConnection(OriginalConnectionString))
            {
                con.Open();
                LOG.Trace($"Inserting objects of type {typeof(T)} from temp_table {TempTableName} to {TableName}");
                using (SqlCommand commandInsert = new SqlCommand("dbo.sp_InsertFromTemp", con))
                {
                    commandInsert.CommandType = CommandType.StoredProcedure;
                    commandInsert.Parameters.Add(new SqlParameter("@tempTableName", SqlDbType.VarChar)).Value = TempTableName.Replace("##", "");
                    commandInsert.Parameters.Add(new SqlParameter("@originTable", SqlDbType.VarChar)).Value = TableName;
                    commandInsert.CommandTimeout = 750;
                    commandInsert.ExecuteNonQuery();
                    LOG.Info("Sucessful inserted items to database");
                }
            }
        }

        private void PerformUpdate()
        {
            using (var con = new SqlConnection(OriginalConnectionString))
            {
                con.Open();
                LOG.Trace($"Updating objects of type {typeof(T)} from temp_table {TempTableName} to {TableName}");
                using (SqlCommand commandUpdate = new SqlCommand("dbo.sp_UpdateFromTempTable", con))
                {
                    commandUpdate.CommandType = CommandType.StoredProcedure;
                    commandUpdate.Parameters.Add(new SqlParameter("@tempTableName", SqlDbType.VarChar)).Value = TempTableName.Replace("##", "");
                    commandUpdate.Parameters.Add(new SqlParameter("@originTable", SqlDbType.VarChar)).Value = TableName;
                    commandUpdate.CommandTimeout = 750;
                    commandUpdate.ExecuteNonQuery();
                    LOG.Info("Sucessful updated items to database");
                }
            }
        }

        ~BatchInsert()
        {
            if (CancellationToken != null)
                CancellationToken.Cancel();

            if (TempTableKeepAlive != null)
            {
                TempTableKeepAlive.Wait();
                TempTableKeepAlive.Dispose();
            }

            if (TempTableKeeper != null)
            {
                TempTableKeeper.Close();
                TempTableKeeper.Dispose();
            }
        }
    }

    public static class ExtensionBatch
    {
        internal static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> input, int blockSize)
        {
            var enumerator = input.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return nextPartition(enumerator, blockSize).ToList();
            }
        }

        internal static IEnumerable<T> nextPartition<T>(IEnumerator<T> enumerator, int blockSize)
        {
            do
            {
                yield return enumerator.Current;
            }
            while (--blockSize > 0 && enumerator.MoveNext());
        }

        internal static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => p.IsDefined(typeof(ColumnAttribute)) && !p.IsDefined(typeof(NotMappedAttribute))).ToArray();
            DataTable table = new DataTable();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                table.Columns.Add(property.Name, property.DiscoveryRealType());
            }

            object[] values = new object[table.Columns.Count];

            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (properties[i].PropertyType.IsGenericType && properties[i].PropertyType.GetGenericTypeDefinition() == typeof(Lazy<>))
                    {
                        var lazy = properties[i].GetValue(item);
                        if (lazy != null)
                            values[i] = lazy.GetType().GetProperty(nameof(Lazy<object>.Value)).GetValue(lazy);
                    }
                    else
                    {
                        values[i] = properties[i].GetValue(item);
                    }
                }
                table.Rows.Add(values);
            }

            return table;
        }

        internal static DataTable ToDataTable<T>(this ReadOnlySpan<T> data, Dictionary<string, PropertyInfo> schemaProperties)
        {
            DataTable table = schemaProperties.CreateDataTable();
            PropertyInfo[] properties = schemaProperties.Select(p => p.Value).ToArray();

            object[] values = new object[table.Columns.Count];
            foreach (T item in data)
            {
                if (item != null)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        if (properties[i].PropertyType.IsGenericType && properties[i].PropertyType.GetGenericTypeDefinition() == typeof(Lazy<>))
                        {
                            var lazy = properties[i].GetValue(item);
                            if (lazy != null)
                                values[i] = lazy.GetType().GetProperty(nameof(Lazy<object>.Value)).GetValue(lazy);
                        }
                        else
                        {
                            values[i] = properties[i].GetValue(item);
                        }
                    }
                    table.Rows.Add(values);
                }
            }

            return table;
        }

        internal static DataTable CreateDataTable(this Dictionary<string, PropertyInfo> properties)
        {
            DataTable table = new DataTable();

            foreach (var property in properties)
            {
                table.Columns.Add(property.Key, property.Value.DiscoveryRealType());
            }

            return table;
        }
    }
}
