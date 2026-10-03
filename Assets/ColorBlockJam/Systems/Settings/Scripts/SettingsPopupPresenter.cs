using System.Threading;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Settings
{
    public sealed class SettingsPopupPresenter : WindowPresenter<SettingsPopup, bool>
    {
        private readonly SettingsToggles toggles;

        public SettingsPopupPresenter(ISettingsService settings, IHapticService haptics)
        {
            toggles = new SettingsToggles(settings, haptics);
        }

        public UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            return OpenAsync(false, cancellationToken).AsUniTask();
        }

        protected override void OnViewCreated()
        {
            toggles.Bind(View);
        }

        protected override void OnViewDestroyed()
        {
            toggles.Unbind(View);
        }

        protected override void OnShowing()
        {
            toggles.Show(View);
        }
    }
}
