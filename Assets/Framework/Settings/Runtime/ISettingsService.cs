using System;

namespace Framework.Settings
{
    public interface ISettingsService
    {
        event Action<SettingKind, bool> Changed;

        bool IsEnabled(SettingKind setting);

        void SetEnabled(SettingKind setting, bool enabled);
    }
}
