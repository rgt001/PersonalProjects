using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SqlServer.Persistor
{
    internal static class Operators
    {
        #region constants
        const string closeParentheses = ") ";
        const string commaSpace = ", ";
        const string singleQuote = "'";
        const string dSingleQuote = "''";
        #endregion

        public static Dictionary<ExpressionType, string> OperatorsCSharp = GetOperatorsCSharp();
        public static Dictionary<ExpressionType, string> OperatorsSQL = GetOperatorsSQL();
        public static Dictionary<string, Func<object[], string>> MethodsToSQL = GetMethodsSQL();

        private static Dictionary<ExpressionType, string> GetOperatorsCSharp()
        {
            Dictionary<ExpressionType, string> temp = new Dictionary<ExpressionType, string>();
            temp.Add(ExpressionType.AndAlso, " && ");
            temp.Add(ExpressionType.OrElse, " || ");
            temp.Add(ExpressionType.Equal, " == ");
            temp.Add(ExpressionType.GreaterThanOrEqual, " >= ");
            temp.Add(ExpressionType.GreaterThan, " > ");
            temp.Add(ExpressionType.LessThanOrEqual, " <= ");
            temp.Add(ExpressionType.LessThan, " < ");
            temp.Add(ExpressionType.NotEqual, " != ");
            return temp;
        }

        private static Dictionary<ExpressionType, string> GetOperatorsSQL()
        {
            Dictionary<ExpressionType, string> temp = new Dictionary<ExpressionType, string>();
            temp.Add(ExpressionType.AndAlso, " AND ");
            temp.Add(ExpressionType.OrElse, " OR ");
            temp.Add(ExpressionType.Equal, " = ");
            temp.Add(ExpressionType.GreaterThanOrEqual, " >= ");
            temp.Add(ExpressionType.GreaterThan, " > ");
            temp.Add(ExpressionType.LessThanOrEqual, " <= ");
            temp.Add(ExpressionType.LessThan, " < ");
            temp.Add(ExpressionType.NotEqual, " <> ");
            return temp;
        }

        private static Dictionary<string, Func<object[], string>> GetMethodsSQL()
        {
            Dictionary<string, Func<object[], string>> temp = new Dictionary<string, Func<object[], string>>();
            temp.Add("IsNullOrWhiteSpace", p => IsNullOrWhiteSpace(p));
            temp.Add("IsNullOrEmpty", p => IsNullOrEmpty(p));
            temp.Add("StartsWith", p => LikeRight(p));
            temp.Add("EndWith", p => LikeLeft(p));
            temp.Add("Equals", p => Equals(p));
            temp.Add("Contains", p => Like(p));
            temp.Add("GetValue", p => GetValue(p));
            temp.Add("AddMinutes", p => AddMinutes(p));
            temp.Add("AddDays", p => AddDays(p));
            temp.Add("AddMonths", p => AddMonths(p));
            temp.Add("AddYears", p => AddYears(p));
            temp.Add("AddHours", p => AddHours(p));
            temp.Add("ConvertSmallDateTime", p => ConvertSmallDateTime(p));

            temp.Add("MAX", p => MAX(p));
            temp.Add("MIN", p => MIN(p));
            temp.Add("AVG", p => AVG(p));
            temp.Add("SUM", p => SUM(p));
            temp.Add("GROUPED", p => GROUPED(p));

            temp.Add("ASC", p => ASC(p));
            temp.Add("DESC", p => DESC(p));

            temp.Add("Parse", p => Parse(p));
            return temp;
        }

        private static string Parse(object[] field)
        {
            return field[0].ToString();
        }

        private static string MAX(object[] field)
        {
            const string command = "MAX(";
            return command + field[0].ToString() +  closeParentheses;
        }

        private static string MIN(object[] field)
        {
            const string command = "MIN(";
            return command + field[0].ToString() + closeParentheses;
        }

        private static string AVG(object[] field)
        {
            const string command = "AVG(";
            return command + field[0].ToString() + closeParentheses;
        }

        private static string SUM(object[] field)
        {
            const string command = "SUM(";
            return command + field[0].ToString() + closeParentheses;
        }

        private static string GROUPED(object[] field)
        {
            return field[0].ToString() + " ";
        }


        private static string ASC(object[] field)
        {
            return field[0].ToString() + " ASC,";
        }

        private static string DESC(object[] field)
        {
            return field[0].ToString() + " DESC,";
        }

        private static string GetValue(object[] field)
        {
            var textString = field[0].ToString();
            if (textString.StartsWith(singleQuote) && textString.EndsWith(singleQuote))
                textString = textString.Substring(1, textString.Length - 2);

            return textString;
        }

        private static string ConvertSmallDateTime(object[] text)
        {
            const string command = "convert(smalldatetime, ";
            var textString = command + text[0].ToString() + closeParentheses;
            return textString;
        }

        private static string AddDays(object[] text)
        {
            const string command = "DATEADD(dd, ";
            var textString = command + text[0].ToString() + commaSpace + text[1].ToString() + closeParentheses;
            return textString;
        }

        private static string AddMonths(object[] text)
        {
            const string command = "DATEADD(MM, ";
            var textString = command + text[0].ToString() + commaSpace + text[1].ToString() + closeParentheses;
            return textString;
        }

        private static string AddYears(object[] text)
        {
            const string command = "DATEADD(yy, ";
            var textString = command + text[0].ToString() + commaSpace + text[1].ToString() + closeParentheses;
            return textString;
        }

        private static string AddHours(object[] text)
        {
            const string command = "DATEADD(hh, ";
            var textString = command + text[0].ToString() + commaSpace + text[1].ToString() + closeParentheses;
            return textString;
        }

        private static string AddMinutes(object[] text)
        {
            const string command = "DATEADD(n, ";
            var textString = command + text[0].ToString() + commaSpace + text[1].ToString() + closeParentheses;
            return textString;
        }

        private static string Equals(object[] text)
        {
            var textString = text[0].ToString();
            if (textString.StartsWith(singleQuote) && textString.EndsWith(singleQuote))
                textString = textString.Substring(1, textString.Length - 2);

            textString = textString.Replace(singleQuote, dSingleQuote);
            return " = '" + textString + singleQuote;
        }

        private static string Like(object[] text)
        {
            var textString = text[0].ToString();
            if (textString.StartsWith(singleQuote) && textString.EndsWith(singleQuote))
                textString = textString.Substring(1, textString.Length - 2);

            textString = textString.Replace(singleQuote, dSingleQuote);
            return " LIKE '%" + textString + "%'";
        }

        private static string LikeLeft(object[] text)
        {
            var textString = text[0].ToString();
            if (textString.StartsWith(singleQuote) && textString.EndsWith(singleQuote))
                textString = textString.Substring(1, textString.Length - 2);

            textString = textString.Replace(singleQuote, dSingleQuote);
            return " LIKE '%" + textString + singleQuote;
        }

        private static string LikeRight(object[] text)
        {
            var textString = text[0].ToString();
            if (textString.StartsWith(singleQuote) && textString.EndsWith(singleQuote))
                textString = textString.Substring(1, textString.Length - 2);

            textString = textString.Replace(singleQuote, dSingleQuote);
            return " LIKE '" + textString + "%'";
        }

        /// <summary>
        /// The expected result is like below
        /// (FIELD is not null and trim(FIELD) <> '')
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string IsNullOrWhiteSpace(object[] field)
        {
            var fieldString = field[0].ToString();
            StringBuilder sb = new StringBuilder();

            sb.Append(" (");
            sb.Append(fieldString);
            sb.Append(" is not null AND trim(");
            sb.Append(fieldString);
            sb.Append(") <> ''");
            sb.Append(closeParentheses);

            return sb.ToString();
        }

        /// <summary>
        /// The expected result is like below
        /// (FIELD is not null and FIELD <> '')
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string IsNullOrEmpty(object[] field)
        {
            var fieldString = field[0].ToString();
            StringBuilder sb = new StringBuilder();

            sb.Append(" (");
            sb.Append(fieldString);
            sb.Append(" is not null AND ");
            sb.Append(fieldString);
            sb.Append(" <> ''");
            sb.Append(closeParentheses);

            return sb.ToString();
        }
    }
}
