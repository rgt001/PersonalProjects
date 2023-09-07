using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using SqlServer.Attributes;
using SqlServer.DataBase;
using Utils;

namespace SqlServer.Persistor
{
    public static partial class LorealPersistor
    {
        private static string[] GetPrimaryKey(string tableName)
        {
            using (Conexao con = new Conexao())
            {
                using (SqlDataAdapter da = new SqlDataAdapter($"Select * from PrimaryKeys where table_name = '{tableName}'", con.Conectar()))
                {
                    using (DataTable dt = new DataTable())
                    {

                        da.Fill(dt);

                        string[] primaryKeys = new string[dt.Rows.Count];

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];
                            primaryKeys[i] = row["column_name"].ToString();
                        }

                        return primaryKeys;
                    }
                }
            }
        }

        private static string TranslateSqlCommandFull(SqlCommand cmd)
        {
            DbType[] quotedParameterTypes = new DbType[] {
                    DbType.AnsiString, DbType.Date, DbType.DateTime,
                    DbType.Guid, DbType.String, DbType.AnsiStringFixedLength,
                    DbType.StringFixedLength };
            string query = cmd.CommandText;

            var arrParams = new SqlParameter[cmd.Parameters.Count];
            cmd.Parameters.CopyTo(arrParams, 0);

            foreach (SqlParameter p in arrParams.OrderByDescending(p => p.ParameterName.Length))
            {
                string value = p.Value?.ToString() ?? null;
                if (value == null)
                    value = "null";

                else if (p.DbType == DbType.Boolean)
                    value = value.AsBit().ToString();

                else if (p.DbType == DbType.DateTime)
                {
                    string date = value.AsDateTime();
                    if (date == null)
                        value = "null";
                    else
                        value = "'" + value.AsDateTime() + "'";
                }

                else if (p.DbType == DbType.DateTimeOffset)
                {
                    string date = value.AsDateTime();
                    if (date == null)
                        value = "null";
                    else
                        value = "CAST('" + value.AsDateTime() + "' AS DATETIMEOFFSET)";
                }

                else if (p.DbType == DbType.Double)
                    value = value.Replace(',', '.');

                //else if (p.SqlDbType == SqlDbType.VarBinary)
                //{
                //    var valueAsByteArray = p.Value as Byte[];
                //    value = valueAsByteArray.ToHexadecimal(true);
                //}

                else if (quotedParameterTypes.Contains(p.DbType))
                    value = "'" + value + "'";

                query = query.Replace(p.ParameterName, value);
            }

            return query;
        }

        private static string TranslateSqlCommand(SqlCommand cmd)
        {
            string query = cmd.CommandText;

            foreach (SqlParameter p in cmd.Parameters)
            {
                query = query.Replace(p.ParameterName, p.Value.ToString());
            }

            return query;
        }

        public static IEnumerable<T> RunSql<T>(string command) where T : new()
        {
            using (Conexao con = new Conexao())
            {
                Type typeOfClass = typeof(T);
                string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;

                if (!command.Contains(tableName))
                    throw new Exception("Fdp detectado");

                using (SqlDataAdapter da = new SqlDataAdapter(command, con.Conectar()))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        T instanceOfClass = (T)FormatterServices.GetUninitializedObject(typeof(T));
                        PropertyInfo[] properties = typeOfClass.GetProperties();

                        foreach (var property in properties)
                            if (row[property.Name].GetType() != typeof(DBNull))
                                property.SetValue(instanceOfClass, row[property.Name]);

                        yield return instanceOfClass;
                    }

                    dt.Dispose();
                    con.Dispose();
                }
            }
        }

        private static string GetParametrizedWhere(string tableName)
        {
            using (Conexao con = new Conexao())
            {
                using (SqlDataAdapter da = new SqlDataAdapter($"Select * from PrimaryKeys where table_name = '{tableName}'", con.Conectar()))
                {
                    using (DataTable dt = new DataTable())
                    {

                        da.Fill(dt);

                        StringBuilder where = new StringBuilder(" where ");
                        foreach (DataRow row in dt.Rows)
                        {
                            where.Append(row["column_name"]);
                            where.Append(" = @");
                            where.Append(row["column_name"]);
                            where.Append(" and ");
                        }
                        where.Remove(where.Length - 5, 5);

                        return where.ToString();
                    }
                }
            }
        }
    }
}