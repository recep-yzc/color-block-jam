using System;
using System.Collections.Generic;
using Framework.Core.Persistence;

namespace Framework.Settings
{
    public sealed class SettingsService : ISettingsService
    {
        private const string KeyPrefix = "settings.";

        private readonly IKeyValueStorage storage;
        private readonly Dictionary<SettingKind, bool> values = new();
        private readonly Dictionary<SettingKind, string> keys = new();

        public SettingsService(IKeyValueStorage storage)
        {
            this.storage = storage;

            foreach (SettingKind setting in Enum.GetValues(typeof(SettingKind)))
            {
                var key = KeyPrefix + setting;
                keys[setting] = key;
                values[setting] = storage.GetBool(key, defaultValue: true);
            }
        }

        public event Action<SettingKind, bool> Changed;

        public bool IsEnabled(SettingKind setting)
        {
            return values[setting];
        }

        public void SetEnabled(SettingKind setting, bool enabled)
        {
            if (values[setting] == enabled)
            {
                return;
            }

            values[setting] = enabled;
            storage.SetBool(keys[setting], enabled);
            Changed?.Invoke(setting, enabled);
        }
    }
}
