using System;

namespace ColorBlockJam.Settings
{
    public interface ISettingsService
    {
        event Action<SettingKind, bool> Changed;

        bool IsEnabled(SettingKind setting);

        void SetEnabled(SettingKind setting, bool enabled);
    }
}
