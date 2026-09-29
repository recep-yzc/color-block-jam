using Framework.UI.Views;

namespace ColorBlockJam.Economy
{
    public sealed class CoinHudPresenter : ViewPresenter<CoinHudView>
    {
        private readonly ICoinWallet wallet;

        public CoinHudPresenter(CoinHudView view, ICoinWallet wallet)
            : base(view)
        {
            this.wallet = wallet;
        }

        protected override void OnInitialize()
        {
            View.SetCoins(wallet.Coins);
            wallet.CoinsChanged += View.SetCoins;
        }

        protected override void OnDispose()
        {
            wallet.CoinsChanged -= View.SetCoins;
        }
    }
}
