using Framework.Pooling;
using UnityEngine;

namespace Framework.Tests
{
    /// <summary>
    /// Counts the pool callbacks it receives.
    /// </summary>
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
