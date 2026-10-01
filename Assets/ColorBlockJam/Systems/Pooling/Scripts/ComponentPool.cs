using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Pooling
{
    public sealed class ComponentPool<T> : IDisposable where T : Component
    {
        private readonly T prefab;
        private readonly Transform root;
        private readonly Func<T, Transform, T> instantiate;
        private readonly ObjectPool<T> pool;
        private readonly Dictionary<T, IPoolable[]> poolables = new();

        public ComponentPool(T prefab, Transform root, Func<T, Transform, T> instantiate = null, int maxSize = 1000)
        {
            this.prefab = prefab;
            this.root = root;
            this.instantiate = instantiate ?? ((original, parent) => Object.Instantiate(original, parent));
            pool = new ObjectPool<T>(Create, OnGet, OnRelease, OnDestroy, collectionCheck: Application.isEditor, maxSize: maxSize);
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
            var instance = instantiate(prefab, root);
            instance.gameObject.SetActive(false);
            poolables.Add(instance, instance.GetComponentsInChildren<IPoolable>(true));
            return instance;
        }

        private void OnGet(T instance)
        {
            instance.gameObject.SetActive(true);
            foreach (var poolable in poolables[instance])
            {
                poolable.OnTakenFromPool();
            }
        }

        private void OnRelease(T instance)
        {
            foreach (var poolable in poolables[instance])
            {
                poolable.OnReturnedToPool();
            }

            instance.gameObject.SetActive(false);
            instance.transform.SetParent(root, false);
        }

        private void OnDestroy(T instance)
        {
            poolables.Remove(instance);

            if (instance != null)
            {
                Object.Destroy(instance.gameObject);
            }
        }
    }
}
