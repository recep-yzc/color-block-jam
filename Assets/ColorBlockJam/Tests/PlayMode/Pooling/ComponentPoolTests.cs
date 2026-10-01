using ColorBlockJam.Pooling;
using NUnit.Framework;
using UnityEngine;

namespace ColorBlockJam.Tests
{
    public sealed class ComponentPoolTests
    {
        private GameObject root;
        private Transform prefab;
        private ComponentPool<Transform> pool;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("PoolRoot");
            prefab = new GameObject("Prefab").transform;
            pool = new ComponentPool<Transform>(prefab, root.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(prefab.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void GetActivatesAndParents()
        {
            var parent = new GameObject("Parent").transform;
            parent.SetParent(root.transform);

            var instance = pool.Get(parent);

            Assert.IsTrue(instance.gameObject.activeSelf);
            Assert.AreEqual(parent, instance.parent);
        }

        [Test]
        public void ReleaseDeactivatesAndReuses()
        {
            var parent = new GameObject("Parent").transform;
            parent.SetParent(root.transform);
            var instance = pool.Get(parent);

            pool.Release(instance);

            Assert.IsFalse(instance.gameObject.activeSelf);
            Assert.AreEqual(root.transform, instance.parent, "A released instance goes back under the pool root.");
            Assert.AreSame(instance, pool.Get(root.transform));
        }

        [Test]
        public void PrewarmCreatesInactiveInstances()
        {
            pool.Prewarm(3);

            Assert.AreEqual(3, pool.CountInactive);
            Assert.AreEqual(3, root.transform.childCount);
        }
    }
}
