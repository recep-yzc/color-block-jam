using ColorBlockJam.Economy;
using ColorBlockJam.Settings;
using ColorBlockJam.Shared.UI.Popups;
using ColorBlockJam.Shared.UI.Views;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Home
{
    public sealed class HomeHudPresenter : ViewPresenter<HomeHudView>
    {
        private readonly IPopupService popups;
        private readonly ICoinWallet wallet;

        public HomeHudPresenter(HomeHudView view, IPopupService popups, ICoinWallet wallet)
            : base(view)
        {
            this.popups = popups;
            this.wallet = wallet;
        }

        protected override void OnInitialize()
        {
            View.SetCoins(wallet.Coins);
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
