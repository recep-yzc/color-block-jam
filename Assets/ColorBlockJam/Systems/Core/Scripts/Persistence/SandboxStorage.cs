using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Core.Persistence
{
    public sealed class SandboxStorage : IKeyValueStorage
    {
        private readonly Dictionary<string, int> values = new();

        public bool GetBool(string key, bool defaultValue)
        {
            return GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        public void SetBool(string key, bool value)
        {
            SetInt(key, value ? 1 : 0);
        }

        public int GetInt(string key, int defaultValue)
        {
            return values.TryGetValue(key, out var value) ? value : PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            values[key] = value;
        }
    }
}
