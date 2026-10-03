namespace ColorBlockJam.Settings
{
    public interface ISettingsService
    {
        bool IsEnabled(SettingKind setting);

        void SetEnabled(SettingKind setting, bool enabled);
    }
}
