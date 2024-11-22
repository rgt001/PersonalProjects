using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
    public class MultiCastObservable<T> : IDisposable, IObservable<T>, IObserver<T> where T : class
    {
        //private static readonly ILog LOG = LogManager.GetLogger("MultiCastObservable." + typeof(T).Name);
        private static readonly dynamic LOG = new object();//Replace for a real log system

        private ImmutableList<IObserver<T>> observers = ImmutableList.Create<IObserver<T>>();
        private readonly ConcurrentQueue<Tuple<int, IObserver<T>, object>> actions = new ConcurrentQueue<Tuple<int, IObserver<T>, object>>();
        private readonly List<Tuple<Task, CancellationTokenSource>> workers = new List<Tuple<Task, CancellationTokenSource>>();
        private readonly ManualResetEventSlim hasWorkEvent = new ManualResetEventSlim(false);
        private readonly BlockingCollection<Action> signalQueue = new BlockingCollection<Action>();
        private readonly object resetLock = new object();
        private readonly object workersLock = new object();
        private readonly Timer logger;

        public bool HasObservers => observers.Count > 0;
        private readonly int minWorkers;
        private readonly int maxWorkers;
        private readonly int queueSizeTrigger;
        private readonly bool hasWorkersRange = false;

        public MultiCastObservable(int workers = 1)
        {
            WorkersNumber = workers;

            logger = new Timer(Logger, null, 15_000, 30_000);

            Task.Run(() => ProcessSignalQueue());
        }

        public MultiCastObservable(int minWorkers, int maxWorkers, int queueSizeTrigger) : this(minWorkers)
        {
            hasWorkersRange = true;
            this.minWorkers = minWorkers;
            this.maxWorkers = maxWorkers;
            this.queueSizeTrigger = queueSizeTrigger;
        }

        private void ProcessSignalQueue()
        {
            foreach (var action in signalQueue.GetConsumingEnumerable())
            {
                action.Invoke();
            }
        }

        internal Tuple<Task, CancellationTokenSource> CreateWorker()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            return new Tuple<Task, CancellationTokenSource>(Worker(source.Token), source);
        }

        private void Logger(object token)
        {
            try
            {
                int queueSize = actions.Count;
                if (queueSize == 0)
                {
                    if (hasWorkersRange && WorkersNumber > minWorkers)
                    {
                        WorkersNumber--;
                        LOG.Info("Worker count decreased, current amount:" + WorkersNumber);
                    }

                    return;
                }

                if (hasWorkersRange && queueSize > queueSizeTrigger && WorkersNumber < maxWorkers)
                {
                    WorkersNumber++;
                    LOG.Info("Worker count increased, current amount:" + WorkersNumber);
                }

                LOG.Info("Queue size:" + queueSize);

                Tuple<int, IObserver<T>, object> mostRecentWork;
                Tuple<int, IObserver<T>, object> mostRecentWork2;

                if (actions.TryPeek(out mostRecentWork))
                {
                    Thread.Sleep(5_000);

                    if (actions.TryPeek(out mostRecentWork2) && mostRecentWork.Item1 == mostRecentWork2.Item1 && mostRecentWork.Item3 == mostRecentWork2.Item3)
                    {
                        LOG.Warn("The queue has the same item at the top");
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private Task Worker(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    hasWorkEvent.Wait(token);

                    if (token.IsCancellationRequested)
                        break;

                    while (actions.TryDequeue(out var work))
                    {
                        try
                        {
                            switch (work.Item1)
                            {
                                case 1:
                                    onCompleted(work.Item2);
                                    break;
                                case 2:
                                    if (work.Item3 is Exception ex)
                                        onError(work.Item2, ex);
                                    break;
                                case 3:
                                    onNext(work.Item2, work.Item3 as T);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.Error($"Observer error while executing --> {work?.Item3}");
                            HandleWorkerException(ex);
                        }
                    }

                    ResetWorkIfNecessary();
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void HandleWorkerException(Exception ex)
        {
            LOG.Error(ex);

            Tuple<Task, CancellationTokenSource> faultyWorker = null;

            lock (workersLock)
            {
                faultyWorker = workers.FirstOrDefault(p => p.Item1.IsFaulted);
                if (faultyWorker != null)
                {
                    workers.Remove(faultyWorker);
                }
            }

            if (faultyWorker != null)
            {
                faultyWorker.Item2.Cancel();
                try
                {
                    faultyWorker.Item1.Wait();
                }
                catch (AggregateException ae)
                {
                    LOG.Warn("Worker terminated with exceptions", ae);
                }

                lock (workersLock)
                {
                    workers.Add(CreateWorker());
                }
            }
        }

        public int WorkersNumber
        {
            get
            {
                lock (workersLock)
                {
                    return workers.Count;
                }
            }
            set
            {
                lock (workersLock)
                {
                    if (value <= 0) return;

                    if (value < workers.Count)
                    {
                        int toRemove = workers.Count - value;

                        for (int i = 0; i < toRemove; i++)
                        {
                            workers[0].Item2.Cancel();
                            workers[0].Item1.Wait();
                            workers.RemoveAt(0);
                        }
                    }
                    else
                    {
                        for (int i = workers.Count; i < value; i++)
                        {
                            workers.Add(CreateWorker());
                        }
                    }
                }
            }
        }

        public void OnCompleted()
        {
            foreach (IObserver<T> obs in observers)
                actions.Enqueue(new Tuple<int, IObserver<T>, object>(1, obs, null));

            SignalWork();
        }

        public void OnError(Exception error)
        {
            foreach (IObserver<T> obs in observers)
                actions.Enqueue(new Tuple<int, IObserver<T>, object>(2, obs, error));

            SignalWork();
        }

        public void OnNext(T value)
        {
            foreach (IObserver<T> obs in observers)
                actions.Enqueue(new Tuple<int, IObserver<T>, object>(3, obs, value));

            SignalWork();
        }

        private void SignalWork()
        {
            signalQueue.Add(() =>
            {
                lock (resetLock)
                {
                    hasWorkEvent.Set();
                }
            });
        }

        private void ResetWorkIfNecessary()
        {
            signalQueue.Add(() =>
            {
                lock (resetLock)
                {
                    if (actions.IsEmpty)
                        hasWorkEvent.Reset();
                }
            });
        }

        internal void onCompleted(IObserver<T> observer)
        {
            observer.OnCompleted();
        }

        internal void onError(IObserver<T> observer, Exception error)
        {
            LOG.Error(error);
            observer.OnError(error);
        }

        internal void onNext(IObserver<T> observer, T value)
        {
            observer.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!observers.Contains(observer))
            {
                observers = observers.Add(observer);
                LOG.Info("Subscribe added");
            }
            else
            {
                LOG.Info("Already subscribed");
            }

            return new Disposer<T>(observer, RemoveSubscribe);
        }

        private void RemoveSubscribe(IObserver<T> observer)
        {
            if (observers.Contains(observer))
            {
                observers = observers.Remove(observer);
                LOG.Info("Subscribe removed");
            }
        }

        public void Dispose()
        {
            signalQueue.CompleteAdding();
            logger.Dispose();

            lock (workersLock)
            {
                foreach (var worker in workers)
                {
                    worker.Item2.Cancel();
                    worker.Item1.Wait();
                }

                workers.Clear();
            }
        }
    }

    internal class Disposer<T> : IDisposable
    {
        private IObserver<T> observer;
        private Action<IObserver<T>> removeSubscribe;

        public Disposer(IObserver<T> observer, Action<IObserver<T>> removeSubscribe)
        {
            this.observer = observer;
            this.removeSubscribe = removeSubscribe;
        }

        public void Dispose()
        {
            removeSubscribe.Invoke(observer);
        }
    }
}