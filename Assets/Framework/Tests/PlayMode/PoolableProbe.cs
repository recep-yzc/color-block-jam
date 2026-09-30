using Framework.Pooling;
using UnityEngine;

namespace Framework.Tests
{
    public sealed class PoolableProbe : MonoBehaviour, IPoolable
    {
        public int TakenCount { get; private set; }
        public int ReturnedCount { get; private set; }

        public void OnTakenFromPool()
        {
            TakenCount++;
        }

        public void OnReturnedToPool()
        {
            ReturnedCount++;
        }
    }
}
