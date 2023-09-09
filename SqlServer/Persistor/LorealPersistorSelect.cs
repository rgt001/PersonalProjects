using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Linq.Expressions;
using SqlServer.Attributes;
using SqlServer.DataBase;
using Utils;

namespace SqlServer.Persistor
{
    public static partial class LorealPersistor
    {
        readonly static Type TypeDbNull = typeof(DBNull);
        const string SelectFrom = "Select * from ";

        public static IEnumerable<T> Select<T>(string where = "", int? top = null, bool noLock = false) where T : new()
        {
            Type typeOfClass = typeof(T);
            string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;
            string select;

            if (noLock)
                select = SelectFrom + tableName + " WITH (NOLOCK) " + where;
            else
                select = SelectFrom + tableName + where;

            if (top != null && top > 0)
                select = select.Replace(SelectFrom, $"Select TOP({top}) * from ");

            DataTable dt = new DataTable();
            using (Conexao con = new Conexao(BootStrap.ConnectionString))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(select, con.Conectar()))
                {
                    da.Fill(dt);
                }
            }

            using (dt)
            {
                Dictionary<PropertyInfo, bool> propTypes = null;

                if (dt.Rows.Count > 0)
                    propTypes = typeOfClass.GetProperties().ToDictionary(p => p, p => Nullable.GetUnderlyingType(p.PropertyType) != null);

                foreach (DataRow row in dt.Rows)
                {
                    T instanceOfClass = (T)FormatterServices.GetUninitializedObject(typeOfClass);

                    foreach (KeyValuePair<PropertyInfo, bool> property in propTypes)
                    {
                        Type currentType = row[property.Key.Name].GetType();
                        if (currentType == TypeDbNull)
                            continue;
                        else if (property.Key.PropertyType != currentType && property.Value == false)
                            property.Key.SetValue(instanceOfClass, Convert.ChangeType(row[property.Key.Name], property.Key.PropertyType));
                        else
                            property.Key.SetValue(instanceOfClass, row[property.Key.Name]);
                    }

                    yield return instanceOfClass;
                }
            }
        }

        public static IEnumerable<T> JoinSelect<T>(string where = "", int? top = null, bool noLock = false) where T : new()
        {
            const string asSpace = " as ";
            Type sqlJoinAttribute = typeof(SqlJoinAttribute);
            Type typeOfClass = typeof(T);
            PropertyInfo[] properties = typeOfClass.GetProperties().Where(p => Attribute.IsDefined(p, sqlJoinAttribute, false)).ToArray();

            Dictionary<Type, Tuple<SqlJoinAttribute, string>> typesAndTables = new Dictionary<Type, Tuple<SqlJoinAttribute, string>>();
            {
                string tempTableName;
                Type tempType;
                SqlJoinAttribute tempJoinType;

                foreach (PropertyInfo property in properties)
                {
                    tempJoinType = property.GetCustomAttribute<SqlJoinAttribute>(false);
                    tempType = property.PropertyType;
                    tempTableName = tempType.GetCustomAttribute<SqlTableNameAttribute>(false).Name;

                    typesAndTables.Add(tempType, new Tuple<SqlJoinAttribute, string>(tempJoinType, tempTableName + asSpace + property.Name));
                }
            }

            string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;
            StringBuilder select = new StringBuilder();

            if (top != null && top > 0)
                select.Append($"Select TOP({top}) * from ");
            else
                select.Append(SelectFrom);

            select.Append(typesAndTables.First().Value.Item2);

            if (noLock)
                select.Append(" WITH (NOLOCK) ");

            Expression<Func<T, bool>> expression;

            foreach (var join in typesAndTables.Skip(1))
            {
                select.Append("\r\n");


                switch (join.Value.Item1.JoinType)
                {
                    case SqlDbJoinType.InnerJoin:
                        select.Append(" inner join ");
                        break;
                    case SqlDbJoinType.OuterJoin:
                        select.Append(" outer join ");
                        break;
                    case SqlDbJoinType.LeftJoin:
                        select.Append(" left join ");
                        break;
                    case SqlDbJoinType.RightJoin:
                        select.Append(" right join ");
                        break;
                    default:
                        break;
                }

                select.Append(join.Value.Item2);

                expression = join.Value.Item1.on.DynamicInvoke() as Expression<Func<T, bool>>;

                select.Append(ExpressionHelper.ToSQLJoinOn(expression));
            }

            select.Append(where);

            DataTable dt = new DataTable();
            using (Conexao con = new Conexao(BootStrap.ConnectionString))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(select.ToString(), con.Conectar()))
                {
                    da.Fill(dt);
                }
            }

            using (dt)
            {
                Dictionary<PropertyInfo, bool> propTypes = null;

                if (dt.Rows.Count > 0)
                    propTypes = typeOfClass.GetProperties().ToDictionary(p => p, p => Nullable.GetUnderlyingType(p.PropertyType) != null);

#warning I will inspect it at my leisure
                //It will be significantly more performatic using this, I need to analyze and fit the code for use this
                //List<Dictionary<PropertyInfo, bool>> PropertiesWithinProperty = GetProperties(typeOfClass, dt, propTypes);

                foreach (DataRow row in dt.Rows)
                {
                    T instanceOfClass = (T)FormatterServices.GetUninitializedObject(typeOfClass);

                    int columnIndex = 0;
                    int end = 0;
                    foreach (KeyValuePair<PropertyInfo, bool> property in propTypes)
                    {
                        object propertyInstance = FormatterServices.GetUninitializedObject(property.Key.PropertyType);

                        PropertyInfo[] propertiesDescription = propertyInstance.GetType().GetProperties();

                        end += propertiesDescription.Length;
                        for (; columnIndex < end; columnIndex++)
                        {
                            DataColumn currentColumn = dt.Columns[columnIndex];

                            PropertyInfo currentProperty = propertiesDescription.FirstOrDefault(p => p.PropertyType == currentColumn.DataType && currentColumn.ColumnName.StartsWith(p.Name));
                            if (currentProperty != null)
                            {
                                Type currentType = row[currentProperty.Name].GetType();
                                if (currentType == TypeDbNull)
                                    continue;
                                else if (currentProperty.PropertyType != currentType && Nullable.GetUnderlyingType(currentProperty.PropertyType) != null)
                                    currentProperty.SetValue(propertyInstance, Convert.ChangeType(row[columnIndex], currentProperty.PropertyType));
                                else
                                    currentProperty.SetValue(propertyInstance, row[columnIndex]);
                            }
                        }

                        property.Key.SetValue(instanceOfClass, propertyInstance);
                    }

                    yield return instanceOfClass;
                }
            }            
        }

        private static List<Dictionary<PropertyInfo, bool>> GetProperties(Type typeOfClass, DataTable dt, Dictionary<PropertyInfo, bool> propTypes)
        {
            if (dt.Rows.Count > 0)
                propTypes = typeOfClass.GetProperties().ToDictionary(p => p, p => Nullable.GetUnderlyingType(p.PropertyType) != null);

            List<Dictionary<PropertyInfo, bool>> propOfProps = new List<Dictionary<PropertyInfo, bool>>();

            foreach (var item in propTypes)
            {
                var tt = StaticInfos.TableColumns.Value[item.Key.PropertyType];
                propOfProps.Add(item.Key.PropertyType.GetProperties().ToDictionary(p => p, p => Nullable.GetUnderlyingType(p.PropertyType) != null));
            }


            return propOfProps;
        }

        public static IEnumerable<T> Select<T>(Expression<Func<T, bool>> filtro, int? top = null, bool noLock = false) where T : new()
        {
            var where = ExpressionHelper.ToSQLWHERE<T>(filtro);

            bool isJoinClass = typeof(T).GetCustomAttribute<SqlTableNameAttribute>(false).Join;

            if (isJoinClass)
                return JoinSelect<T>(where, top, noLock);
            else
                return Select<T>(where, top, noLock);
        }

        private static int Count<T>(string where = "", bool noLock = false) where T : new()
        {
            const string selectFrom = "Select count(*) from ";
            using (Conexao con = new Conexao(BootStrap.ConnectionString))
            {
                Type typeOfClass = typeof(T);
                string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;
                string select;

                if (noLock)
                    select = selectFrom + tableName + " WITH (NOLOCK) " + where;
                else
                    select = selectFrom + tableName + where;

                int count = 0;
                using (SqlCommand cmd = new SqlCommand(select, con.Conectar()))
                {
                    count = (int)cmd.ExecuteScalar();

                    cmd.Dispose();
                    con.Dispose();
                }

                return count;
            }
        }

        public static int Count<T>(Expression<Func<T, bool>> filtro, bool noLock = false) where T : new()
        {
            var where = ExpressionHelper.ToSQLWHERE<T>(filtro);

            return Count<T>(where, noLock);
        }

        /// <summary>
        /// Equivalente ao Top 1
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public static T GetFirst<T>(Expression<Func<T, bool>> filtro, bool noLock = false) where T : new()
        {
            var where = ExpressionHelper.ToSQLWHERE<T>(filtro);

            return Select<T>(where, 1, noLock).FirstOrDefault();
        }

        public static bool Exists<T>(T source) where T : new()
        {
            using (Conexao con = new Conexao(BootStrap.ConnectionString))
            {
                Type typeOfClass = typeof(T);
                string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;
                string[] pks = GetPrimaryKey(tableName);

                StringBuilder where = new StringBuilder(" where ");
                using (SqlCommand cmd = new SqlCommand("Select * from " + tableName, con.Conectar()))
                {
                    for (int i = 0; i < pks.Length; i++)
                    {
                        if (i > 0)
                        {
                            where.Append(" and ");
                        }

                        PropertyInfo tempType = typeOfClass.GetProperties().Where(p => p.Name.ToLower() == pks[i].ToLower()).FirstOrDefault();

                        where.Append($"{pks[i]} = @{pks[i]}");

                        cmd.Parameters.Add($"@{pks[i]}", tempType.PropertyType.AsSqlType()).Value = tempType.GetValue(source);
                    }

                    cmd.CommandText += where.ToString();

                    if (cmd.ExecuteScalar() == null)
                        return false;

                    return true;
                }
            }
        }

    }
}