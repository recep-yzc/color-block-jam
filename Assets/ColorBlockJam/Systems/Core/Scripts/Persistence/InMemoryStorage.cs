using System.Collections.Generic;

namespace ColorBlockJam.Core.Persistence
{
    public sealed class InMemoryStorage : IKeyValueStorage
    {
        private readonly Dictionary<string, int> values = new();

        public bool GetBool(string key, bool defaultValue)
        {
            return values.TryGetValue(key, out var value) ? value != 0 : defaultValue;
        }

        public void SetBool(string key, bool value)
        {
            values[key] = value ? 1 : 0;
        }

        public int GetInt(string key, int defaultValue)
        {
            return values.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void SetInt(string key, int value)
        {
            values[key] = value;
        }
    }
}
