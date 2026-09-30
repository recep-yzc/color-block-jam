using Framework.UI.Views;

namespace Framework.Settings
{
    public sealed class SettingsPopupPresenter : ViewPresenter<SettingsPopup>
    {
        private readonly ISettingsService settings;
        private readonly IHapticService haptics;

        public SettingsPopupPresenter(SettingsPopup popup, ISettingsService settings, IHapticService haptics)
            : base(popup)
        {
            this.settings = settings;
            this.haptics = haptics;
        }

        protected override void OnInitialize()
        {
            foreach (var toggle in View.Toggles)
            {
                toggle.Changed += OnToggleChanged;
            }
        }

        protected override void OnDispose()
        {
            foreach (var toggle in View.Toggles)
            {
                toggle.Changed -= OnToggleChanged;
            }
        }

        protected override void OnShowing()
        {
            foreach (var toggle in View.Toggles)
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
