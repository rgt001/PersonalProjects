using SqlServer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Models
{
    [SqlTableName(true)]
    public class AggregateCategoryDiary : IWhereFunction
    {
        [SqlJoin(SqlDbJoinType.Principal, typeof(AggregateCategoryDiary))]
        public DiaryModel Diary { get; set; }

        [SqlJoin(SqlDbJoinType.InnerJoin, typeof(AggregateCategoryDiary))]
        public CategoryModel Category { get; set; }

        public static Expression<Func<T, bool>> JoinCategory<T>() where T : AggregateCategoryDiary
        {
            return (p => p.Diary.CategoryID == p.Category.ID);
        }

        public Delegate GetJoinOn(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Category):
                    return new GetJoinOnFor<AggregateCategoryDiary>(JoinCategory<AggregateCategoryDiary>);
                default:
                    return null;
                    break;
            }

            throw new NotImplementedException();
        }
    }
}
