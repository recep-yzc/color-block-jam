using System.Threading;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Settings
{
    public class SettingsPopupPresenter : WindowPresenter<SettingsPopup>
    {
        private readonly ISettingsService settings;
        private readonly IHapticService haptics;

        public SettingsPopupPresenter(ISettingsService settings, IHapticService haptics)
        {
            this.settings = settings;
            this.haptics = haptics;
        }

        public UniTask<SettingsChoice> ShowAsync(CancellationToken cancellationToken = default)
        {
            return OpenAsync(SettingsChoice.Close, cancellationToken);
        }

        protected override void OnViewCreated()
        {
            foreach (var toggle in View.Toggles)
            {
                toggle.Changed += OnToggleChanged;
            }

            if (View.HomeButton != null)
            {
                View.HomeButton.Clicked += OnHomeClicked;
            }
        }

        protected override void OnViewDestroyed()
        {
            foreach (var toggle in View.Toggles)
            {
                toggle.Changed -= OnToggleChanged;
            }

            if (View.HomeButton != null)
            {
                View.HomeButton.Clicked -= OnHomeClicked;
            }
        }

        protected override void OnShowing()
        {
            foreach (var toggle in View.Toggles)
            {
                toggle.Show(settings.IsEnabled(toggle.Setting));
            }
        }

        private void OnHomeClicked()
        {
            Finish(SettingsChoice.Home);
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
