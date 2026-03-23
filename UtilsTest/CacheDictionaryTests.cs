using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Util;

namespace UtilTests
{
    [TestClass]
    public class CacheDictionaryTests
    {
        private static readonly int[] TestSeeds =
        {
            1,
            7,
            13,
            42,
            123,
            999,
            2024,
            77777
        };

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Snapshot_ShouldContainAllItems()
        {
            CacheDictionary<int, string> cache = new CacheDictionary<int, string>();

            for (int i = 0; i < 10_000; i++)
                cache.Add(i, "V" + i);

            IReadOnlyDictionary<int, string> snapshot = cache.GetAccessibleOnes();

            Assert.AreEqual(10_000, snapshot.Count);

            for (int i = 0; i < 10_000; i++)
            {
                Assert.IsTrue(snapshot.TryGetValue(i, out string value));
                Assert.AreEqual("V" + i, value);
            }
        }

        [TestMethod]
        public void Snapshot_ShouldNotContainRemovedItems()
        {
            CacheDictionary<int, string> cache = new CacheDictionary<int, string>();

            for (int i = 0; i < 10_000; i++)
                cache.Add(i, "V" + i);

            for (int i = 0; i < 5_000; i++)
                cache.Remove(i);

            IReadOnlyDictionary<int, string> snapshot = cache.GetAccessibleOnes();

            Assert.AreEqual(5_000, snapshot.Count);

            for (int i = 0; i < 5_000; i++)
                Assert.IsFalse(snapshot.ContainsKey(i));

            for (int i = 5_000; i < 10_000; i++)
            {
                Assert.IsTrue(snapshot.TryGetValue(i, out string value));
                Assert.AreEqual("V" + i, value);
            }
        }

        [TestMethod]
        public void Snapshot_ShouldMatchReferenceDictionary_AfterMixedOperations()
        {
            CacheDictionary<int, string> cache = new CacheDictionary<int, string>();
            Dictionary<int, string> expected = new Dictionary<int, string>();

            for (int i = 0; i < 20_000; i++)
            {
                cache.AddOrUpdate(i, "A" + i);
                expected[i] = "A" + i;
            }

            for (int i = 0; i < 10_000; i += 2)
            {
                cache.Remove(i);
                expected.Remove(i);
            }

            for (int i = 5_000; i < 15_000; i++)
            {
                cache.AddOrUpdate(i, "B" + i);
                expected[i] = "B" + i;
            }

            IReadOnlyDictionary<int, string> snapshot = cache.GetAccessibleOnes();

            Assert.AreEqual(expected.Count, snapshot.Count);

            foreach (KeyValuePair<int, string> pair in expected)
            {
                Assert.IsTrue(snapshot.TryGetValue(pair.Key, out string value), "Missing key " + pair.Key);
                Assert.AreEqual(pair.Value, value, "Unexpected value for key " + pair.Key);
            }

            KeyValuePair<int, string>[] expectedOrdered = expected.OrderBy(x => x.Key).ToArray();
            KeyValuePair<int, string>[] snapshotOrdered = snapshot.OrderBy(x => x.Key).ToArray();

            Assert.AreEqual(expectedOrdered.Length, snapshotOrdered.Length);

            for (int i = 0; i < expectedOrdered.Length; i++)
            {
                Assert.AreEqual(expectedOrdered[i].Key, snapshotOrdered[i].Key);
                Assert.AreEqual(expectedOrdered[i].Value, snapshotOrdered[i].Value);
            }
        }

        [TestMethod]
        public void OldSnapshot_ShouldRemainStructurallyStable_AfterFurtherWrites()
        {
            CacheDictionary<int, string> cache = new CacheDictionary<int, string>();

            for (int i = 0; i < 1_000; i++)
                cache.Add(i, "V" + i);

            IReadOnlyDictionary<int, string> oldSnapshot = cache.GetAccessibleOnes();

            for (int i = 1_000; i < 2_000; i++)
                cache.Add(i, "V" + i);

            for (int i = 0; i < 100; i++)
                cache.Remove(i);

            Assert.AreEqual(1_000, oldSnapshot.Count);

            for (int i = 0; i < 1_000; i++)
            {
                Assert.IsTrue(oldSnapshot.TryGetValue(i, out string value));
                Assert.AreEqual("V" + i, value);
            }

            IReadOnlyDictionary<int, string> newSnapshot = cache.GetAccessibleOnes();
            Assert.AreEqual(1_900, newSnapshot.Count);
        }

        [TestMethod]
        public void Snapshot_ShouldBeShallowCopy_ForReferenceValues()
        {
            CacheDictionary<int, List<int>> cache = new CacheDictionary<int, List<int>>();
            List<int> sharedList = new List<int> { 1, 2, 3 };

            cache.Add(1, sharedList);

            IReadOnlyDictionary<int, List<int>> snapshot1 = cache.GetAccessibleOnes();

            sharedList.Add(4);

            IReadOnlyDictionary<int, List<int>> snapshot2 = cache.GetAccessibleOnes();

            Assert.AreEqual(4, snapshot1[1].Count);
            Assert.AreEqual(4, snapshot2[1].Count);
        }

        [TestMethod]
        public void ClearAll_ShouldResetCacheState()
        {
            CacheDictionary<int, string> cache = new CacheDictionary<int, string>();

            for (int i = 0; i < 50_000; i++)
                cache.Add(i, "V" + i);

            cache.GetAccessibleOnes();

            cache.ClearAll();

            IReadOnlyDictionary<int, string> snapshot = cache.GetAccessibleOnes();

            Assert.AreEqual(0, snapshot.Count);

            cache.Add(1, "A");

            snapshot = cache.GetAccessibleOnes();

            Assert.AreEqual(1, snapshot.Count);
            Assert.AreEqual("A", snapshot[1]);
        }

        [TestMethod]
        public void Enumeration_ShouldMatchLookupResults()
        {
            CacheDictionary<int, string> cache = new CacheDictionary<int, string>();

            for (int i = 0; i < 15_000; i++)
                cache.Add(i, "V" + i);

            for (int i = 0; i < 3_000; i++)
                cache.Remove(i);

            IReadOnlyDictionary<int, string> snapshot = cache.GetAccessibleOnes();

            foreach (KeyValuePair<int, string> pair in snapshot)
            {
                Assert.IsTrue(snapshot.TryGetValue(pair.Key, out string value));
                Assert.AreEqual(pair.Value, value);
            }
        }

        [TestMethod]
        public void HandleCollectionAdd_ShouldCreateCollection_WhenKeyDoesNotExist()
        {
            CacheDictionary<int, List<int>> cache = new CacheDictionary<int, List<int>>();

            cache.HandleCollectionAdd<int, List<int>>(10, 123);

            IReadOnlyDictionary<int, List<int>> snapshot = cache.GetAccessibleOnes();

            Assert.AreEqual(1, snapshot.Count);
            Assert.IsTrue(snapshot.ContainsKey(10));

            CollectionAssert.AreEqual(new[] { 123 }, snapshot[10].ToArray());
        }

        [TestMethod]
        public void HandleCollectionAdd_ShouldAppendToExistingCollection()
        {
            CacheDictionary<int, List<int>> cache = new CacheDictionary<int, List<int>>();

            cache.Add(1, new List<int> { 10, 20 });
            cache.HandleCollectionAdd<int, List<int>>(1, 30);

            IReadOnlyDictionary<int, List<int>> snapshot = cache.GetAccessibleOnes();

            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, snapshot[1].ToArray());
        }

        [TestMethod]
        public void MultipleSnapshots_ShouldRemainUsable_AcrossRebuilds()
        {
            CacheDictionary<int, string> cache = new CacheDictionary<int, string>();

            for (int i = 0; i < 100; i++)
                cache.Add(i, "A" + i);

            IReadOnlyDictionary<int, string> snapshot1 = cache.GetAccessibleOnes();

            for (int i = 100; i < 200; i++)
                cache.Add(i, "B" + i);

            IReadOnlyDictionary<int, string> snapshot2 = cache.GetAccessibleOnes();

            for (int i = 0; i < 50; i++)
                cache.Remove(i);

            IReadOnlyDictionary<int, string> snapshot3 = cache.GetAccessibleOnes();

            Assert.AreEqual(100, snapshot1.Count);
            Assert.AreEqual(200, snapshot2.Count);
            Assert.AreEqual(150, snapshot3.Count);

            Assert.IsTrue(snapshot1.ContainsKey(10));
            Assert.IsTrue(snapshot2.ContainsKey(150));
            Assert.IsFalse(snapshot3.ContainsKey(10));
        }

        [TestMethod]
        public void RepeatedRemoveAndReuse_ShouldRemainConsistent()
        {
            CacheDictionary<int, int> cache = new CacheDictionary<int, int>();
            Dictionary<int, int> expected = new Dictionary<int, int>();

            for (int cycle = 0; cycle < 200; cycle++)
            {
                for (int i = 0; i < 2_000; i++)
                {
                    int value = cycle * 10_000 + i;
                    cache.AddOrUpdate(i, value);
                    expected[i] = value;
                }

                for (int i = 0; i < 2_000; i += 3)
                {
                    cache.Remove(i);
                    expected.Remove(i);
                }
            }

            IReadOnlyDictionary<int, int> snapshot = cache.GetAccessibleOnes();

            Assert.AreEqual(expected.Count, snapshot.Count);

            foreach (KeyValuePair<int, int> pair in expected)
            {
                Assert.IsTrue(snapshot.TryGetValue(pair.Key, out int value));
                Assert.AreEqual(pair.Value, value);
            }
        }

        [TestMethod]
        public void ConcurrentReadsAndWrites_ShouldNotBreakPublishedSnapshots()
        {
            CacheDictionary<int, int> cache = new CacheDictionary<int, int>();

            for (int i = 0; i < 10_000; i++)
                cache.Add(i, i);

            List<Task> tasks = new List<Task>();

            for (int reader = 0; reader < 4; reader++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < 20_000; i++)
                    {
                        IReadOnlyDictionary<int, int> snapshot = cache.GetAccessibleOnes();

                        foreach (KeyValuePair<int, int> pair in snapshot.Take(50))
                        {
                            Assert.IsTrue(snapshot.TryGetValue(pair.Key, out int value));
                            Assert.AreEqual(pair.Value, value);
                        }
                    }
                }));
            }

            for (int writer = 0; writer < 2; writer++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < 20_000; i++)
                    {
                        cache.AddOrUpdate(i % 15_000, i);

                        if (i % 5 == 0)
                            cache.Remove((i + 123) % 15_000);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }

        [TestMethod]
        public void Stress_MixedOperations_ShouldProduceConsistentSnapshot_ForAllSeeds()
        {
            foreach (int seed in TestSeeds)
            {
                RunStressMixedOperationsScenario(seed);
            }
        }

        [TestMethod]
        public void Randomized_RemoveReuse_ShouldRemainConsistent_ForAllSeeds()
        {
            foreach (int seed in TestSeeds)
            {
                RunRandomizedRemoveReuseScenario(seed);
            }
        }

        private void RunStressMixedOperationsScenario(int seed)
        {
            CacheDictionary<int, int> cache = new CacheDictionary<int, int>();
            Dictionary<int, int> expected = new Dictionary<int, int>();
            Random random = new Random(seed);

            for (int i = 0; i < 100_000; i++)
            {
                int key = random.Next(0, 10_000);
                int operation = random.Next(0, 3);

                switch (operation)
                {
                    case 0:
                        cache.AddOrUpdate(key, i);
                        expected[key] = i;
                        break;

                    case 1:
                        cache.Remove(key);
                        expected.Remove(key);
                        break;

                    default:
                        cache.AddOrUpdate(key, -i);
                        expected[key] = -i;
                        break;
                }
            }

            IReadOnlyDictionary<int, int> snapshot = cache.GetAccessibleOnes();

            Assert.AreEqual(expected.Count, snapshot.Count, "Seed: " + seed);

            foreach (KeyValuePair<int, int> pair in expected)
            {
                Assert.IsTrue(snapshot.TryGetValue(pair.Key, out int value),
                    "Seed " + seed + " - Missing key " + pair.Key);

                Assert.AreEqual(pair.Value, value,
                    "Seed " + seed + " - Unexpected value for key " + pair.Key);
            }

            TestContext?.WriteLine("Stress scenario passed for seed: " + seed);
        }

        private void RunRandomizedRemoveReuseScenario(int seed)
        {
            CacheDictionary<int, int> cache = new CacheDictionary<int, int>();
            Dictionary<int, int> expected = new Dictionary<int, int>();
            Random random = new Random(seed);

            for (int cycle = 0; cycle < 200; cycle++)
            {
                for (int i = 0; i < 5_000; i++)
                {
                    int key = random.Next(0, 3_000);
                    int value = random.Next();

                    cache.AddOrUpdate(key, value);
                    expected[key] = value;
                }

                for (int i = 0; i < 2_000; i++)
                {
                    int key = random.Next(0, 3_000);
                    cache.Remove(key);
                    expected.Remove(key);
                }
            }

            IReadOnlyDictionary<int, int> snapshot = cache.GetAccessibleOnes();

            Assert.AreEqual(expected.Count, snapshot.Count, "Seed: " + seed);

            foreach (KeyValuePair<int, int> pair in expected)
            {
                Assert.IsTrue(snapshot.TryGetValue(pair.Key, out int value),
                    "Seed " + seed + " - Missing key " + pair.Key);

                Assert.AreEqual(pair.Value, value,
                    "Seed " + seed + " - Unexpected value for key " + pair.Key);
            }

            TestContext?.WriteLine("Remove/reuse scenario passed for seed: " + seed);
        }
    }
}