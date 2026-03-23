using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Util
{
    /// <summary>
    /// Gets return a structural snapshot of the current dictionary state.
    /// Later structural changes (add, remove, update) will not be reflected in
    /// an already published snapshot.
    ///
    /// Values are not deep-copied. If TValue is a mutable reference type,
    /// internal changes inside those objects may still be visible through
    /// previously published snapshots.
    /// </summary>
    public class CacheDictionary<TKey, TValue>
    {
        private readonly FieldInfo entriesField;
        private readonly FieldInfo countField;
        private readonly FieldInfo freeCountField;
        private readonly FieldInfo versionField;
        private readonly FieldInfo bucketsField;

        private Dictionary<TKey, TValue> InternalDictionary = new Dictionary<TKey, TValue>();
        private Dictionary<TKey, TValue> AccessibleDictionary = new Dictionary<TKey, TValue>();

        private volatile bool itsTrustable = false;

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly object _cacheLock = new object();
        private readonly object _collectionMutationLock = new object();

        public CacheDictionary()
        {
            entriesField = typeof(Dictionary<TKey, TValue>).GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic);
            countField = typeof(Dictionary<TKey, TValue>).GetField("count", BindingFlags.Instance | BindingFlags.NonPublic);
            freeCountField = typeof(Dictionary<TKey, TValue>).GetField("freeCount", BindingFlags.Instance | BindingFlags.NonPublic);
            versionField = typeof(Dictionary<TKey, TValue>).GetField("version", BindingFlags.Instance | BindingFlags.NonPublic);
            bucketsField = typeof(Dictionary<TKey, TValue>).GetField("buckets", BindingFlags.Instance | BindingFlags.NonPublic);

            if (entriesField is null ||
                countField is null ||
                freeCountField is null ||
                versionField is null ||
                bucketsField is null)
            {
                throw new NotSupportedException(
                    "CacheDictionary depends on internal Dictionary<TKey, TValue> fields that were not found in the current runtime.");
            }
        }

        public bool ItsTrustable => itsTrustable;

        public void Add(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                InternalDictionary.Add(key, value);
                itsTrustable = false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(TKey key)
        {
            _lock.EnterWriteLock();
            try
            {
                InternalDictionary.Remove(key);
                itsTrustable = false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddOrUpdate(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                // Intentionally kept as Remove + Add.
                // In benchmark tests on .NET Framework 4.7.2, this approach showed
                // an average gain of ~8% over InternalDictionary[key] = value
                // in this specific workload.
                InternalDictionary.Remove(key);
                InternalDictionary.Add(key, value);

                itsTrustable = false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void HandleCollectionAdd<TItem, TCollection>(TKey key, TItem item)
            where TCollection : TValue, ICollection<TItem>, new()
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (InternalDictionary.TryGetValue(key, out TValue existing))
                {
                    if (existing is TCollection collection)
                    {
                        lock (_collectionMutationLock)
                        {
                            collection.Add(item);
                        }

                        itsTrustable = false;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"The existing value for key '{key}' is not of the expected type '{typeof(TCollection).Name}'.");
                    }
                }
                else
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        if (!InternalDictionary.TryGetValue(key, out existing))
                        {
                            TCollection newCollection = new TCollection();
                            newCollection.Add(item);
                            InternalDictionary.Add(key, newCollection);
                            itsTrustable = false;
                        }
                        else if (existing is TCollection existingCollection)
                        {
                            lock (_collectionMutationLock)
                            {
                                existingCollection.Add(item);
                            }

                            itsTrustable = false;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"The existing value for key '{key}' is not of the expected type '{typeof(TCollection).Name}'.");
                        }
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public TValue Get(TKey key)
        {
            TryGet(key, out TValue result);
            return result;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            var accessible = GetAccessibleOnes();
            return accessible.TryGetValue(key, out value);
        }

        public void ClearAll()
        {
            _lock.EnterWriteLock();
            try
            {
                if (InternalDictionary.Count < 10_000)
                    InternalDictionary.Clear();
                else
                    InternalDictionary = new Dictionary<TKey, TValue>();

                AccessibleDictionary = new Dictionary<TKey, TValue>();
                itsTrustable = false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public int Count()
        {
            var accessible = GetAccessibleOnes();
            return accessible.Count;
        }

        public IReadOnlyDictionary<TKey, TValue> GetAccessibleOnes()
        {
            if (itsTrustable)
                return AccessibleDictionary;

            _lock.EnterUpgradeableReadLock();
            try
            {
                if (!itsTrustable)
                {
                    lock (_cacheLock)
                    {
                        if (!itsTrustable)
                        {
                            AccessibleDictionary = CopyDictionaryWithoutKeyVerification(InternalDictionary);
                            itsTrustable = true;
                        }
                    }
                }

                return AccessibleDictionary;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        private Dictionary<TKey, TValue> CopyDictionaryWithoutKeyVerification(Dictionary<TKey, TValue> original)
        {
            if (original.Count > 0)
            {
                Array entries = Unsafe.As<Array>(entriesField.GetValue(original));
                Array buckets = Unsafe.As<Array>(bucketsField.GetValue(original));
                Dictionary<TKey, TValue> newDict = new Dictionary<TKey, TValue>(buckets.Length);

                if (entries != null)
                {
                    Array.Copy(entries, Unsafe.As<Array>(entriesField.GetValue(newDict)), entries.Length);
                    Array.Copy(buckets, Unsafe.As<Array>(bucketsField.GetValue(newDict)), buckets.Length);

                    countField.SetValue(newDict, countField.GetValue(original));
                    versionField.SetValue(newDict, versionField.GetValue(original));
                    freeCountField.SetValue(newDict, newDict.Count - original.Count);
                }

                return newDict;
            }

            return new Dictionary<TKey, TValue>(original.Count);
        }
    }
}