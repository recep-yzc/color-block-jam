using ColorBlockJam.Economy;
using Framework.UI.Views;

namespace ColorBlockJam.Gameplay
{
    public sealed class OutOfTimePopupPresenter : ViewPresenter<OutOfTimePopup>
    {
        private readonly LevelSession session;
        private readonly ICoinWallet wallet;
        private readonly GameplayConfig config;
        private bool hasBoughtTime;

        public OutOfTimePopupPresenter(OutOfTimePopup popup, LevelSession session, ICoinWallet wallet, GameplayConfig config)
            : base(popup)
        {
            this.session = session;
            this.wallet = wallet;
            this.config = config;
        }

        protected override void OnInitialize()
        {
            View.ContinueButton.Clicked += OnContinueClicked;
            wallet.CoinsChanged += OnCoinsChanged;
        }

        protected override void OnDispose()
        {
            View.ContinueButton.Clicked -= OnContinueClicked;
            wallet.CoinsChanged -= OnCoinsChanged;
        }

        protected override void OnShowing()
        {
            hasBoughtTime = false;
            ShowOffer();
        }

        protected override void OnHidden()
        {
            if (!hasBoughtTime)
            {
                session.DeclineExtraTime();
            }
        }

        private void OnContinueClicked()
        {
            if (hasBoughtTime || !wallet.TrySpend(config.ExtraTimeCost))
            {
                return;
            }

            hasBoughtTime = true;
            session.AddExtraTime(config.ExtraTimeSeconds);
            View.RequestClose();
        }

        private void OnCoinsChanged(int coins)
        {
            if (View.IsVisible)
            {
                ShowOffer();
            }
        }

        private void ShowOffer()
        {
            View.SetOffer(config.ExtraTimeSeconds, config.ExtraTimeCost, wallet.Coins >= config.ExtraTimeCost);
        }
    }
}
