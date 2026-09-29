using Cysharp.Threading.Tasks;
using Framework.Settings;
using Framework.UI.Popups;
using Framework.UI.Views;

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
