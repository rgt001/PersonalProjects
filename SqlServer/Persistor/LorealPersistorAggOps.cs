using SqlServer.Attributes;
using SqlServer.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.Persistor
{
    public class LorealPersistorAggOps
    {
        private static IEnumerable<T> SelectAggregate<T>(string fields, string where = "", int? top = null, bool noLock = false) where T : new()
        {
            const string selectFrom = "Select * from ";
            Type typeDbNull = typeof(DBNull);
            using (Conexao con = new Conexao(BootStrap.ConnectionString))
            {
                Type typeOfClass = typeof(T);
                string tableName = typeOfClass.GetCustomAttribute<SqlTableNameAttribute>(false).Name;
                string select;

                if (noLock)
                    select = selectFrom + tableName + " WITH (NOLOCK) " + where;
                else
                    select = selectFrom + tableName + where;

                if (top != null && top > 0)
                    select = select.Replace(selectFrom, $"Select TOP({top}) * from ");

                if (fields != null)
                    select = select.Replace("*", fields);

                using (SqlDataAdapter da = new SqlDataAdapter(select, con.Conectar()))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    T instanceOfClass;
                    Type currentType;
                    Dictionary<PropertyInfo, bool> propTypes = null;

                    if (dt.Rows.Count > 0)
                        propTypes = GetPropertyWithOrder(fields, typeOfClass).ToDictionary(p => p, p => Nullable.GetUnderlyingType(p.PropertyType) != null);

                    int count = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        instanceOfClass = (T)FormatterServices.GetUninitializedObject(typeOfClass);
                        foreach (KeyValuePair<PropertyInfo, bool> property in propTypes)
                        {
                            currentType = row[count].GetType();
                            if (currentType == typeDbNull)
                                continue;
                            else if (property.Key.PropertyType != currentType && property.Value == false)
                                property.Key.SetValue(instanceOfClass, Convert.ChangeType(row[count], property.Key.PropertyType));
                            else
                                property.Key.SetValue(instanceOfClass, row[count]);

                            count++;
                        }

                        yield return instanceOfClass;
                        count = 0;
                    }

                    dt.Dispose();
                    con.Dispose();
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetPropertyWithOrder(string fields, Type typeOfClass)
        {
            var props = typeOfClass.GetProperties();
            var t = fields.Split(new string[] { "AS a," }, StringSplitOptions.None);

            foreach (var item in t)
                yield return props.FirstOrDefault(p => item.Contains(p.Name));

            //return typeOfClass.GetProperties().Where(p => fields.Contains(p.Name));
        }


        public static IEnumerable<T> SelectGroupBy<T>(Expression<Func<T, bool>> fieldos, Expression<Func<T, bool>> filtro = null, Expression<Func<T, object>> GroupBy = null, int? top = null, bool noLock = false) where T : new()
        {
            //LorealPersistorAggOps.SelectGroupBy<AgendamentoModel>(p => p.Id_usuario.MAX(),p => p.Id_usuario > 0, (p) => new { p.Tp_TocarAlarme, p.Id_usuario, p.Ds_Descricao }).ToList();
            var fields = ExpressionHelper.ToSQLField<T>(fieldos);
            var where = ExpressionHelper.ToSQLWHERE<T>(filtro);
            var group = ExpressionHelper.ToSQLGroup<T>(GroupBy);

            return SelectAggregate<T>(fields, where + group, top, noLock);
        }

        public static IEnumerable<T> SelectOrderBy<T>(Expression<Func<T, bool>> filtro, Expression<Func<T, bool>> OrderBy, int? top = null, bool noLock = false) where T : new()
        {
            var where = ExpressionHelper.ToSQLWHERE<T>(filtro);
            var group = ExpressionHelper.ToSQLOrderBy<T>(OrderBy);

            return LorealPersistor.Select<T>(where + group, top, noLock);
        }
    }
}
