using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.LorealPersistorHelpers
{
    public static class ToListOf
    {
        public static IEnumerable<TResult> CastTo<T, TResult>(this IEnumerable<T> source) where TResult : ICopyFrom<T>, new()
        {
            TResult temp;
            foreach (var item in source)
            {
                temp = new TResult();
                temp.CopyFrom(item);

                yield return temp;
            }
        }
    }
}
