using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Data;
using Utils;

namespace SqlServer.Persistor
{
    internal static class ExpressionHelper
    {
        const string quotationMark = "\"";
        const string sSingleQuote = "'";
        const char cQuotationMark = '\"';
        const char cSingleQuote = '\'';
        const char dot = '.';
        const char comma = ',';
        const char parenthesesopen = '(';
        const char parenthesesclose = ')';

        readonly static Type typeOfDateTime = typeof(DateTime);
        readonly static Type typeOfString = typeof(string);
        readonly static Type typeOfChar = typeof(char);

        public static string ToSQLSet<T>(Dictionary<string, object> setValues)
        {
            StringBuilder result = new StringBuilder(" set ");
            foreach (var value in setValues)
            {
                result.Append(value.Key);
                result.Append(" = ");

                var type = value.Value.GetType();

                if (!type.TryGetSqlType(out SqlDbType sqlType))
                    throw new NotImplementedException("Unkown type");

                if (value.Value == null)
                    result.Append("null");
                else
                    switch (sqlType)
                    {
                        case SqlDbType.BigInt:
                        case SqlDbType.Binary:
                        case SqlDbType.Money:
                        case SqlDbType.Float:
                        case SqlDbType.Decimal:
                        case SqlDbType.Real:
                        case SqlDbType.Int:
                        case SqlDbType.SmallInt:
                        case SqlDbType.SmallMoney:
                        case SqlDbType.TinyInt:
                            result.Append(value.Value.ToString().Replace(comma, dot));
                            break;
                        case SqlDbType.Bit:
                            if ((bool)value.Value == true)
                                result.Append("1");
                            else
                                result.Append("0");
                            break;
                        case SqlDbType.Char:
                        case SqlDbType.NText:
                        case SqlDbType.NChar:
                        case SqlDbType.NVarChar:
                        case SqlDbType.UniqueIdentifier:
                        case SqlDbType.Text:
                        case SqlDbType.VarChar:
                            result.Append(value.Value.NormalizeString());
                            break;
                        case SqlDbType.DateTime:
                        case SqlDbType.SmallDateTime:
                        case SqlDbType.DateTime2:
                        case SqlDbType.Date:
                        case SqlDbType.DateTimeOffset:
                            result.Append(cSingleQuote + value.Value.AsSqlDateTime() + cSingleQuote);
                            break;
                        case SqlDbType.Time:
                        case SqlDbType.Timestamp:
                            break;
                        default:
                            break;
                    }

                result.Append(" , ");
            }

            return result.Remove(result.Length - 3, 3).ToString();
        }

        public static string ToSQLGroup<T>(Expression<Func<T, object>> source)
        {
            if (source == null)
                return String.Empty;

            Dictionary<int, Expression> expressions = new Dictionary<int, Expression>();
            Dictionary<int, ExpressionType> expressionsRelations = new Dictionary<int, ExpressionType>();

            ResolveExpression(source.Body, expressions, expressionsRelations);

            KeyValuePair<int, string> currentValue;
            StringBuilder result = new StringBuilder(" group by ");
            foreach (var expression in expressions)
            {
                currentValue = SolveExpressions(expression);

                if (currentValue.Key != -1 && currentValue.Value != null)
                    result.Append(currentValue.Value.ToString() + comma);
            }

            return result.Remove(result.Length - 1, 1).ToString();
        }

        public static string ToSQLOrderBy<T>(Expression<Func<T, bool>> source)
        {
            Dictionary<int, Expression> expressions = new Dictionary<int, Expression>();
            Dictionary<int, ExpressionType> expressionsRelations = new Dictionary<int, ExpressionType>();

            ResolveExpression(source.Body, expressions, expressionsRelations);

            KeyValuePair<int, string> currentValue;
            StringBuilder result = new StringBuilder(" order by ");
            foreach (var expression in expressions)
            {
                currentValue = SolveExpressions(expression);

                if (currentValue.Key != -1 && currentValue.Value != null)
                    result.Append(currentValue.Value.ToString());
            }

            return result.Remove(result.Length - 1, 1).ToString();
        }

        public static string ToSQLField<T>(Expression<Func<T, bool>> source)
        {
            Dictionary<int, Expression> expressions = new Dictionary<int, Expression>();
            Dictionary<int, ExpressionType> expressionsRelations = new Dictionary<int, ExpressionType>();

            ResolveExpression(source.Body, expressions, expressionsRelations);

            KeyValuePair<int, string> currentValue;
            StringBuilder result = new StringBuilder(" ");
            foreach (var expression in expressions)
            {
                currentValue = SolveExpressions(expression);

                if (currentValue.Key != -1 && currentValue.Value != null)
                    result.Append(currentValue.Value.ToString() + "AS a" + comma);
            }

            return result.Remove(result.Length - 1, 1).ToString();
        }

        public static string ToSQLWHERE<T>(Expression<Func<T, bool>> source)
        {
            if (source == null)
                return string.Empty;


            Dictionary<int, Expression> expressions = new Dictionary<int, Expression>();
            Dictionary<int, ExpressionType> expressionsRelations = new Dictionary<int, ExpressionType>();
            Dictionary<int, string> solvedExpressions = new Dictionary<int, string>();
            int CurrentInterator = 0;

            ResolveExpression(source.Body, expressions, expressionsRelations);

            KeyValuePair<int, string> currentValue;
            foreach (var expression in expressions)
            {
                currentValue = SolveExpressions(expression);

                if (currentValue.Key != -1 && currentValue.Value != null)
                    solvedExpressions.Add(currentValue.Key, parenthesesopen + currentValue.Value + parenthesesclose);
            }

            StringBuilder result = new StringBuilder(" where ");

            for (int i = 0; i < solvedExpressions.Count; i++)
            {
                result.Append(solvedExpressions[i]);

                if (i < expressionsRelations.Count)
                    result.Append(Operators.OperatorsSQL[expressionsRelations[i]]);
            }

            return result.ToString();
        }

        public static string ToSQLJoinOn<T>(Expression<Func<T, bool>> source)
        {
            if (source == null)
                return string.Empty;

            Dictionary<int, Expression> expressions = new Dictionary<int, Expression>();
            Dictionary<int, ExpressionType> expressionsRelations = new Dictionary<int, ExpressionType>();
            Dictionary<int, string> solvedExpressions = new Dictionary<int, string>();

            ResolveExpression(source.Body, expressions, expressionsRelations);

            foreach (var expression in expressions)
            {
                var sqlOperator = Operators.OperatorsSQL[expression.Value.NodeType];

                if (expression.Value is BinaryExpression binaryExpression)
                {
                    string left = binaryExpression.Left.ToString();
                    string right = binaryExpression.Right.ToString();

                    left = left.Substring(left.IndexOf('.') + 1);
                    right = right.Substring(right.IndexOf('.') + 1);

                    solvedExpressions.Add(expression.Key, left + sqlOperator + right);
                }
            }

            StringBuilder result = new StringBuilder(" on ");
            for (int i = 0; i < solvedExpressions.Count; i++)
            {
                result.Append(solvedExpressions[i]);

                if (i < expressionsRelations.Count)
                    result.Append(Operators.OperatorsSQL[expressionsRelations[i]]);
            }

            return result.ToString();
        }

        private static KeyValuePair<int, string> SolveExpressions(KeyValuePair<int, Expression> source)
        {
            if (source.Value is BinaryExpression binaryExpression)
            {
                object left = GetValueMemberExpression(binaryExpression.Left, source.Value.NodeType);
                object right = GetValueMemberExpression(binaryExpression.Right, source.Value.NodeType);
                string result = left + Operators.OperatorsSQL[source.Value.NodeType] + right;

                return new KeyValuePair<int, string>(source.Key, left + Operators.OperatorsSQL[source.Value.NodeType] + right);
            }
            else if (source.Value is MethodCallExpression methodExpression)
            {
                Func<object[], string> sqlMethod = Operators.MethodsToSQL[methodExpression.Method.Name];

                bool implementedMethod = Operators.MethodsToSQL.TryGetValue(methodExpression.Method.Name, out sqlMethod);
                if (implementedMethod)
                {
                    //If methodExpression.Object == null, is method call only, need to execute or translate
                    if (methodExpression.Object == null)
                    {
                        string method = sqlMethod.Invoke(new object[] { GetValueMemberExpression(methodExpression.Arguments[0], source.Value.NodeType) });

                        return new KeyValuePair<int, string>(source.Key, method);
                    }
                    else
                    {
                        string caller = GetValueMemberExpression(methodExpression.Object, source.Value.NodeType).ToString();
                        string method = sqlMethod.Invoke(new object[] { GetValueMemberExpression(methodExpression.Arguments[0], source.Value.NodeType), caller });

                        if (method.Contains(caller))
                            return new KeyValuePair<int, string>(source.Key, method);
                        else
                            return new KeyValuePair<int, string>(source.Key, caller + method);
                    }
                }
                else
                {
                    //It would be awesome to call the method in C# using invoke, but for now, the idea is to implement it as SQL for the basics, so I have to let it run
                    //because if I do it now, it will be difficult to track when to use a regular method, as everything would be executed here.
                    throw new NotSupportedException("Translate not implemented");
                }


            }
            else if (source.Value is MemberExpression memberExpression)
            {
                return new KeyValuePair<int, string>(source.Key, GetValueMemberExpression(memberExpression, memberExpression.NodeType).ToString());
            }

            return new KeyValuePair<int, string>(-1, null);
        }

        private static void ResolveExpression(Expression expression, Dictionary<int, Expression> expressions, Dictionary<int, ExpressionType> expressionsRelations)
        {
            if (expression is MethodCallExpression methodExpression)
            {
                expressions.Add(expressions.Count, methodExpression);
            }
            else if (expression is BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.AndAlso || binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    expressionsRelations.Add(expressions.Count, expression.NodeType);
                    expressions.Add(expressions.Count, binaryExpression.Right);
                    ResolveExpression(binaryExpression.Left, expressions, expressionsRelations);
                }
                else
                {
                    expressions.Add(expressions.Count, binaryExpression);
                }
            }
            else if (expression is NewExpression newExpression)
            {
                foreach (var args in newExpression.Arguments)
                {
                    expressions.Add(expressions.Count, args);
                    ResolveExpression(args, expressions, expressionsRelations);
                }
            }
        }

        private static object GetValueMemberExpression(Expression expression, ExpressionType nodeType)
        {
            const string isNotNull = "is not null";
            const string isNull = "is null";

            if (expression.NodeType == ExpressionType.Convert && expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.Operand.Type == typeOfChar)
                {
                    string field = ((MemberExpression)unaryExpression.Operand).GetValue().ToString();
                    return " ASCII(" + field + ") ";
                }
                return ((MemberExpression)unaryExpression.Operand).GetValue().ToString();
            }
            else if (TryResolveMemberExpression(expression, out MemberExpression resultLeft))
            {
                return resultLeft.GetValue();
            }
            else if (expression is MethodCallExpression methodExpression)
            {
                return SolveExpressions(new KeyValuePair<int, Expression>(0, methodExpression)).Value;
            }
            else if (expression.NodeType == ExpressionType.Constant)
            {
                string value = expression.ToString();

                if (value == null)
                    if (nodeType == ExpressionType.NotEqual)
                        return isNotNull;
                    else
                        return isNull;


                return value.Replace(sSingleQuote, "''").Replace(cQuotationMark, cSingleQuote);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static bool TryResolveMemberExpression(Expression expression, out MemberExpression result)
        {
            if (expression is MemberExpression memberExpression)
            {
                result = memberExpression;
                return true;
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                result = (MemberExpression)unaryExpression.Operand;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static object GetValue(this MemberExpression exp)
        {
            if (exp.Expression is ConstantExpression)
            {
                var asFather = ((ConstantExpression)exp.Expression).Value;
                var type = asFather.GetType();

                if (exp.NodeType == ExpressionType.MemberAccess)
                {
                    return MemberAccessHandler(exp, asFather);
                }
                throw new NotImplementedException();
            }
            else if (exp.Expression is MemberExpression)
            {
                var result = GetValue((MemberExpression)exp.Expression);
                var fatherExpression = exp.ToString();
                var sonExpression = exp.Expression.ToString();
                Type resultType = result.GetType();
                if (resultType.IsValueType || fatherExpression != sonExpression)
                {
                    IEnumerable<char> remnant = fatherExpression.Skip(sonExpression.Length + 1);
                    var remnantAsString = new string(remnant.ToArray());

                    if (exp.Expression.Type == typeOfDateTime)
                    {
                        switch (remnantAsString)
                        {
                            case "Year":
                                return "Year(" + result + ")";
                            case "Day":
                                return "Day(" + result + ")";
                            case "Month":
                                return "Month(" + result + ")";
                            case "Date":
                                var resultAsString = result.ToString();
                                if (resultAsString.Length > 11 && resultAsString[11] == ' ' && resultAsString[resultAsString.Length - 1] == 39)
                                {
                                    resultAsString = resultAsString.Substring(0, 11) + cSingleQuote;
                                }

                                return resultAsString;
                            default:
                                break;
                        }
                    }
                    else if (!string.IsNullOrEmpty(remnantAsString))
                    {
                        result = MemberAccessHandler(exp, result);
                    }
                }
                else if (resultType == typeOfString)
                {
                    return result.NormalizeString();
                }
                else if (resultType.IsClass)
                {
                    return MemberAccessHandler(exp, result);
                }

                return result;
            }
            else if (exp.Expression != null && exp.Expression.NodeType == ExpressionType.Parameter)
            {
                return exp.ToString().Replace(exp.Expression.ToString() + dot, "");
            }
            else if (exp is MemberExpression)
            {
                if (exp.NodeType == ExpressionType.MemberAccess)
                {
                    return MemberAccessHandler(exp, null);
                }

                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static object MemberAccessHandler(MemberExpression exp, object obj)
        {
            if (exp.Type == typeOfDateTime)
            {
                switch (exp.ToString())
                {
                    case "DateTime.Now":
                    case "DateTime.Today":
                        return "GETDATE()";
                    default:
                        break;
                }
            }

            if (exp.NodeType == ExpressionType.MemberAccess)
            {
                if (obj == null && exp.Expression?.NodeType == ExpressionType.Convert)
                {
                    var property = ((PropertyInfo)exp.Member).Name;
                    return property;
                }
                else if (exp.Member.MemberType == MemberTypes.Property)
                {
                    var property = ((PropertyInfo)exp.Member);
                    return MemberInfoHandler(property.GetValue(obj), property.PropertyType);
                }
                else if (exp.Member.MemberType == MemberTypes.Field)
                {
                    var field = ((FieldInfo)exp.Member);
                    return MemberInfoHandler(field.GetValue(obj), field.FieldType);
                }
            }

            throw new NotImplementedException();
        }

        private static object MemberInfoHandler(object value, Type memberType)
        {
            if (memberType == typeOfString)
            {
                return value.NormalizeString();
            }
            else if (memberType == typeOfDateTime)
            {
                return cSingleQuote + value.AsSqlDateTime() + cSingleQuote;
            }

            return value;
        }

        private static object NormalizeString(this object value)
        {
            if (value is string sValue)
            {
                return cSingleQuote + sValue.Replace(sSingleQuote, "''") + cSingleQuote;
            }

            return value;
        }
    }
}
