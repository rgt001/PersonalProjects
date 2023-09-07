using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

public delegate Expression<Func<T, bool>> GetJoinOnFor<T>();

namespace SqlServer.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlColumnAttribute : Attribute
    {
        private readonly string asName;
        public string AsName => asName;

        public SqlColumnAttribute(string asName)
        {
            this.asName = asName;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SqlTableNameAttribute : Attribute
    {
        private readonly string name;
        public string Name => name;

        private readonly bool join;
        public bool Join => join;

        public SqlTableNameAttribute(string name)
        {
            this.name = name;
        }

        public SqlTableNameAttribute(bool join)
        {
            this.join = join;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SqlJoinAttribute : Attribute
    {
        private readonly SqlDbJoinType joinType;
        public SqlDbJoinType JoinType => joinType;

        public readonly Delegate on;

        public SqlJoinAttribute(SqlDbJoinType joinType, Type classType, [CallerMemberName] string propertyName = null)
        {
            this.joinType = joinType;

            var tempInstance = (IWhereFunction)FormatterServices.GetUninitializedObject(classType);
            this.on = tempInstance.GetJoinOn(propertyName);
        }
    }

    public interface IWhereFunction
    {
        Delegate GetJoinOn(string propertyName);
    }

    [Flags]
    public enum SqlDbJoinType
    {
        Principal = 0x00,
        InnerJoin = 0x01,
        OuterJoin = 0x02,
        LeftJoin = 0x04,
        RightJoin = 0x08,
    }
}


