using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Pooling
{
    public sealed class ComponentPool<T> : IDisposable where T : Component
    {
        private readonly T prefab;
        private readonly Transform root;
        private readonly ObjectPool<T> pool;

        public ComponentPool(T prefab, Transform root)
        {
            this.prefab = prefab;
            this.root = root;
            pool = new ObjectPool<T>(Create, OnGet, OnRelease, OnDestroy, collectionCheck: Application.isEditor);
        }

        public int CountInactive => pool.CountInactive;

        public void Prewarm(int count)
        {
            var instances = new T[count];
            for (var i = 0; i < count; i++)
            {
                instances[i] = pool.Get();
            }

            foreach (var instance in instances)
            {
                pool.Release(instance);
            }
        }

        public T Get(Transform parent)
        {
            var instance = pool.Get();
            instance.transform.SetParent(parent, false);
            return instance;
        }

        public void Release(T instance)
        {
            pool.Release(instance);
        }

        public void Dispose()
        {
            pool.Dispose();
        }

        private T Create()
        {
            var instance = Object.Instantiate(prefab, root);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private static void OnGet(T instance)
        {
            instance.gameObject.SetActive(true);
        }

        private void OnRelease(T instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(root, false);
        }

        private static void OnDestroy(T instance)
        {
            if (instance != null)
            {
                Object.Destroy(instance.gameObject);
            }
        }
    }
}
