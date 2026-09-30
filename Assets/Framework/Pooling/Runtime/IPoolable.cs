namespace Framework.Pooling
{
    public interface IPoolable
    {
        void OnTakenFromPool();

        void OnReturnedToPool();
    }
}
