namespace Framework.Pooling
{
    /// <summary>
    /// Put on any component of a pooled prefab to reset it between uses.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called after the object is taken from the pool and activated.</summary>
        void OnTakenFromPool();

        /// <summary>Called before the object is deactivated and returned to the pool.</summary>
        void OnReturnedToPool();
    }
}
