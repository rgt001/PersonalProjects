using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;
using SqlServer.Attributes;
using SqlServer.DataBase;
using Utils;

namespace SqlServer.Persistor
{
    public static partial class LorealPersistor
    {
        public static void Insert<T>(T source) where T : new()
        {
            Type typeOfClass = typeof(T);
            string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;
            typeOfClass.GetFields();
            string separator = ", ";
            string valueSeparator = ", @";

            using (SqlCommand cmd = new SqlCommand())
            {
                using (Conexao con = new Conexao())
                {
                    StringBuilder command = new StringBuilder($"Insert into {tableName} (");
                    StringBuilder values = new StringBuilder("(@");

                    PropertyInfo[] propertyInfos = typeOfClass.GetProperties();
                    for (int i = 0; i < propertyInfos.Length; i++)
                    {
                        PropertyInfo item = propertyInfos[i];

                        if (item.GetCustomAttribute<IdentityColumnAttribute>() != null)
                        {
                            continue;
                        }

                        command.Append(item.Name);
                        command.Append(separator);

                        values.Append(item.Name);
                        values.Append(valueSeparator);

                        object value = item.GetValue(source);
                        if (value == null)
                            value = DBNull.Value;

                        cmd.Parameters.Add($"@{item.Name}", item.PropertyType.AsSqlType()).Value = value;
                    }

                    command.Remove(command.Length - 2, 2);
                    command.Append(") values ");

                    values.Remove(values.Length - 3, 3);
                    values.Append(")");

                    cmd.CommandText = command.ToString() + values.ToString();

                    try
                    {
                        cmd.Connection = con.Conectar();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    { throw ex; }
                    finally
                    {
                        cmd.Connection.Close();
                    }
                }
            }
        }

        public static void InsertOrUpdate<T>(T source) where T : new()
        {
            if (Exists(source))
                Update(source);
            else
                Insert(source);
        }

        public static void Update<T>(T source) where T : new()
        {
            Type typeOfClass = typeof(T);
            string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;
            typeOfClass.GetFields();
            string separator = " = @";
            string end = ", ";

            using (SqlCommand cmd = new SqlCommand())
            {
                using (Conexao con = new Conexao())
                {
                    StringBuilder command = new StringBuilder($"Update {tableName} set ");

                    PropertyInfo[] propertyInfos = typeOfClass.GetProperties();
                    for (int i = 0; i < propertyInfos.Length; i++)
                    {
                        PropertyInfo item = propertyInfos[i];

                        if (item.GetCustomAttribute<IdentityColumnAttribute>() != null)
                        {
                            cmd.Parameters.Add($"@{item.Name}", item.PropertyType.AsSqlType()).Value = item.GetValue(source);
                            continue;
                        }

                        command.Append(item.Name);
                        command.Append(separator);
                        command.Append(item.Name);
                        command.Append(end);

                        cmd.Parameters.Add($"@{item.Name}", item.PropertyType.AsSqlType()).Value = item.GetValue(source) ?? DBNull.Value;
                    }

                    command.Remove(command.Length - 2, 2);
                    command.Append(' ');

                    cmd.CommandText = command.ToString() + GetParametrizedWhere(tableName);

                    var t = TranslateSqlCommand(cmd);

                    cmd.Connection = con.Conectar();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
        }

        public static void DynamicUpdate<T>(Dictionary<string, object> setValues, Expression<Func<T, bool>> filtro) where T : new()
        {
            Type typeOfClass = typeof(T);
            string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;
            string update = "update " + tableName + ExpressionHelper.ToSQLSet<T>(setValues) + ExpressionHelper.ToSQLWHERE<T>(filtro);
        }

        public static void Delete<T>(T source) where T : new()
        {
            Type typeOfClass = typeof(T);
            string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;

            using (SqlCommand cmd = new SqlCommand())
            {
                using (Conexao con = new Conexao())
                {
                    string[] primaryKeys = GetPrimaryKey(tableName);

                    Dictionary<string, PropertyInfo> properties = typeOfClass.GetProperties().ToDictionary(p => p.Name.ToLower(), p => p);
                    foreach (string pk in primaryKeys)
                    {
                        PropertyInfo property = properties[pk.ToLower()]; 
                        cmd.Parameters.Add($"@{pk}", property.PropertyType.AsSqlType()).Value = property.GetValue(source);
                    }

                    cmd.Connection = con.Conectar();
                    cmd.CommandText = $"Delete from {tableName} " + GetParametrizedWhere(tableName);
                    cmd.ExecuteNonQuery();
                    con.Desconectar();
                }
            }
        }
    }
}