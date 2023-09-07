using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.CollectionExtensions;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections;

namespace Utils.Threads
{
    public static class ThreadExtension
    {
        public static async Task<List<T>> AsyncStartNewSplitedWork<T, Twork>(this List<Twork> workpool, Func<List<Twork>, CancellationToken, List<T>> workToDo, int workerThreads, CancellationToken? token = null)
        {
            var result = await Task.Factory.StartNew(() => StartNewSplitedWork(workpool, workToDo, workerThreads, token));
            return result;
        }

        public static List<T> StartNewSplitedWork<T, Twork>(this List<Twork> workpool, Func<List<Twork>, CancellationToken, List<T>> workToDo, int workerThreads, CancellationToken? token = null)
        {
            var workList = workpool.PartitionFor(workerThreads);
            var enumerator = workList.GetEnumerator();
            List<T> result = new List<T>();
            Task<List<T>>[] threads = new Task<List<T>>[workerThreads];

            TaskFactory<List<T>> factory;
            if (token == null)
            {
                factory = new TaskFactory<List<T>>();
                token = new CancellationToken();
            }
            else
                factory = new TaskFactory<List<T>>(token.Value);

            for (int i = 0; i < workerThreads; i++)
            {
                enumerator.MoveNext();
                List<Twork> currentWork = enumerator.Current;
                threads[i] = factory.StartNew(() => workToDo(currentWork, token.Value));
            }

            Task.WaitAll(threads);

            foreach (var threadResult in threads)
            {
                result.AddRange(threadResult.Result);
            }

            return result;
        }
    }
}
