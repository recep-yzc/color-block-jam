using ColorBlockJam.Settings;
using ColorBlockJam.Shared.UI.Popups;
using ColorBlockJam.Shared.UI.Views;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Home
{
    public sealed class HomeHudPresenter : ViewPresenter<HomeHudView>
    {
        private readonly IPopupService popups;

        public HomeHudPresenter(HomeHudView view, IPopupService popups)
            : base(view)
        {
            this.popups = popups;
        }

        protected override void OnInitialize()
        {
            View.SettingsButton.Clicked += OnSettingsClicked;
        }

        protected override void OnDispose()
        {
            View.SettingsButton.Clicked -= OnSettingsClicked;
        }

        private void OnSettingsClicked()
        {
            popups.ShowAsync<SettingsPopup>().Forget();
        }
    }
}
