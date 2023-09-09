using SqlServer.Attributes;
using SqlServer.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer
{
    public static class StaticInfos
    {
        //public static Lazy<Dictionary<string, int>> TableColumnCount = new Lazy<Dictionary<string, int>>(GetTableAndColumns, true);

        public static Lazy<Dictionary<Type, List<PropertyInfo>>> TableColumns = new Lazy<Dictionary<Type, List<PropertyInfo>>>(GetTableAndColumns, true);

        //private static Dictionary<string,int> GetTableAndColumns()
        //{
        //    const string command = "SELECT TABLE_NAME, COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS group by TABLE_NAME";

        //    Dictionary<string,int> tableAndColumns = new Dictionary<string,int>();
        //    using (Conexao con = new Conexao())
        //    {
        //        using (SqlDataAdapter da = new SqlDataAdapter(command, con.Conectar()))
        //        {
        //            DataTable dt = new DataTable();
        //            da.Fill(dt);

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                tableAndColumns.Add(row[0].ToString(), (int)row[1]);
        //            }
        //        }
        //    }

        //    return tableAndColumns;
        //}

        private static Dictionary<Type, List<PropertyInfo>> GetTableAndColumns()
        {
            const string command = "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS";
            Dictionary<string, List<string>> tableAndColumns = new Dictionary<string, List<string>>();
            using (Conexao con = new Conexao(BootStrap.ConnectionString))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(command, con.Conectar()))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    string table;
                    List<string> columns;
                    foreach (DataRow row in dt.Rows)
                    {
                        table = row[0].ToString();
                        if (tableAndColumns.TryGetValue(table, out columns))
                        {
                            columns.Add(row[1].ToString());
                        }
                        else
                        {
                            columns = new List<string>();
                            columns.Add(row[1].ToString());

                            tableAndColumns.Add(table, columns);
                        }
                    }
                }
            }

            var classes = GetTypesWithSqlAttribute();
            Dictionary<Type, List<PropertyInfo>> bah = new Dictionary<Type, List<PropertyInfo>>();

            List<PropertyInfo> applicableProperties;
            foreach (KeyValuePair<string, List<Type>> tableClasses in classes)
            {
                foreach (var currentClass in tableClasses.Value)
                {
                    applicableProperties = new List<PropertyInfo>();
                    var allProps = currentClass.GetProperties().ToDictionary(p =>
                    {
                        var customName = p.GetCustomAttribute<SqlColumnAttribute>();
                        if (customName != null)
                            return customName.AsName;
                        else
                            return p.Name;
                    }, p => p);

                    foreach (var column in tableAndColumns[tableClasses.Key])
                    {
                        if (allProps.TryGetValue(column, out PropertyInfo currentValue))
                            applicableProperties.Add(currentValue);
                    }

                    bah.Add(currentClass, applicableProperties);
                }
            }

            return bah;
            //return tableAndColumns;
        }

        private static Dictionary<string, List<Type>> GetTypesWithSqlAttribute()
        {
            Dictionary<string, List<Type>> temp = new Dictionary<string, List<Type>>();
            string table;
            List<Type> classes;
            SqlTableNameAttribute att;
            foreach (Type item in AppDomain.CurrentDomain.GetAssemblies().SelectMany(p => p.GetTypes()))
            {
                att = item.GetCustomAttribute<SqlTableNameAttribute>(true);
                if (att != null)
                {
                    table = att.Name;
                    if (table != null)
                    {
                        if (temp.TryGetValue(table, out classes))
                        {
                            classes.Add(item);
                        }
                        else
                        {
                            classes = new List<Type>();
                            classes.Add(item);

                            temp.Add(table, classes);
                        }
                    }
                }

            }

            return temp;
        }
    }
}
