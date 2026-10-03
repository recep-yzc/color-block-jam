namespace ColorBlockJam.Settings
{
    public sealed class SettingsToggles
    {
        private readonly ISettingsService settings;
        private readonly IHapticService haptics;

        public SettingsToggles(ISettingsService settings, IHapticService haptics)
        {
            this.settings = settings;
            this.haptics = haptics;
        }

        public void Bind(SettingsPopup view)
        {
            foreach (var toggle in view.Toggles)
            {
                toggle.Changed += OnToggleChanged;
            }
        }

        public void Unbind(SettingsPopup view)
        {
            foreach (var toggle in view.Toggles)
            {
                toggle.Changed -= OnToggleChanged;
            }
        }

        public void Show(SettingsPopup view)
        {
            foreach (var toggle in view.Toggles)
            {
                toggle.Show(settings.IsEnabled(toggle.Setting));
            }
        }

        private void OnToggleChanged(SettingKind setting, bool isOn)
        {
            settings.SetEnabled(setting, isOn);

            if (setting == SettingKind.Haptic && isOn)
            {
                haptics.Play();
            }
        }
    }
}
