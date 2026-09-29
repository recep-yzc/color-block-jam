using Framework.Pooling;
using NUnit.Framework;
using UnityEngine;

namespace Framework.Tests
{
    public sealed class ComponentPoolTests
    {
        private GameObject root;
        private PoolableProbe prefab;
        private ComponentPool<PoolableProbe> pool;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("PoolRoot");
            prefab = new GameObject("Prefab").AddComponent<PoolableProbe>();
            pool = new ComponentPool<PoolableProbe>(prefab, root.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(prefab.gameObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void GetActivatesAndNotifies()
        {
            var parent = new GameObject("Parent").transform;
            parent.SetParent(root.transform);

            var instance = pool.Get(parent);

            Assert.IsTrue(instance.gameObject.activeSelf);
            Assert.AreEqual(parent, instance.transform.parent);
            Assert.AreEqual(1, instance.TakenCount);
        }

        [Test]
        public void ReleaseDeactivatesNotifiesAndReuses()
        {
            var instance = pool.Get(root.transform);

            pool.Release(instance);

            Assert.IsFalse(instance.gameObject.activeSelf);
            Assert.AreEqual(1, instance.ReturnedCount);
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
