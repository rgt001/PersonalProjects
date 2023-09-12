using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils.LorealPersistorHelpers;
using Utils.Threads;

namespace Utils.CollectionExtensions
{
    public static class SortableExtension
    {
        public static SortableBindingList<TResult> ToSortable<T, TResult>(this IEnumerable<T> source) where TResult : ICopyFrom<T>, new()
        {
            SortableBindingList<TResult> result = new SortableBindingList<TResult>();
            TResult temp;
            foreach (var item in source)
            {
                temp = new TResult();
                temp.CopyFrom(item);
                result.Add(temp);
            }
            return result;
        }

        public static SortableBindingList<TResult> ToSortableMulti<T, TResult>(this List<T> source) where TResult : ICopyFrom<T>, new()
        {
            List<TResult> CopyFrom(List<T> workPool, CancellationToken token = new CancellationToken())
            {
                List<TResult> output = new List<TResult>();
                foreach (var work in workPool)
                {
                    TResult temp = new TResult();
                    temp.CopyFrom(work);
                    output.Add(temp);
                }

                return output;
            }

            List<TResult> result = ThreadExtension.StartNewSplitedWork(source, CopyFrom, Environment.ProcessorCount);

            return new SortableBindingList<TResult>(result);
        }
    }
}
