using System.Threading;
using ColorBlockJam.Settings;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public sealed class PauseMenuPresenter : WindowPresenter<SettingsPopup, PauseChoice>
    {
        private readonly SettingsToggles toggles;
        private PauseMenu menu;

        public PauseMenuPresenter(ISettingsService settings, IHapticService haptics)
        {
            toggles = new SettingsToggles(settings, haptics);
        }

        public UniTask<PauseChoice> ShowAsync(CancellationToken cancellationToken = default)
        {
            return OpenAsync(PauseChoice.Resume, cancellationToken);
        }

        protected override void OnViewCreated()
        {
            toggles.Bind(View);
            menu = View.GetComponent<PauseMenu>();
            menu.HomeButton.Clicked += OnHomeClicked;
        }

        protected override void OnViewDestroyed()
        {
            toggles.Unbind(View);
            menu.HomeButton.Clicked -= OnHomeClicked;
            menu = null;
        }

        protected override void OnShowing()
        {
            toggles.Show(View);
        }

        private void OnHomeClicked()
        {
            Finish(PauseChoice.Home);
        }
    }
}
