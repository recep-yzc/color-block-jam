namespace Framework.Core.Persistence
{
    /// <summary>
    /// Small persistent key-value store for player data such as settings, coins and progress.
    /// </summary>
    public interface IKeyValueStorage
    {
        bool GetBool(string key, bool defaultValue);
        void SetBool(string key, bool value);

        int GetInt(string key, int defaultValue);
        void SetInt(string key, int value);
    }
}
