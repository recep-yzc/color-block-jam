namespace ColorBlockJam.Core.Persistence
{
    public interface IKeyValueStorage
    {
        bool GetBool(string key, bool defaultValue);
        void SetBool(string key, bool value);

        int GetInt(string key, int defaultValue);
        void SetInt(string key, int value);
    }
}
