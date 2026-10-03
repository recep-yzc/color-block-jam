using System.Threading;
using ColorBlockJam.Economy;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public sealed class OutOfTimePopupPresenter : WindowPresenter<OutOfTimePopup, bool>
    {
        private readonly ICoinWallet wallet;
        private ExtraTimeOffer offer;

        public OutOfTimePopupPresenter(ICoinWallet wallet)
        {
            this.wallet = wallet;
        }

        public UniTask<bool> ShowAsync(ExtraTimeOffer extraTime, CancellationToken cancellationToken = default)
        {
            offer = extraTime;
            return OpenAsync(false, cancellationToken);
        }

        protected override void OnViewCreated()
        {
            View.ContinueButton.Clicked += OnContinueClicked;
            wallet.CoinsChanged += OnCoinsChanged;
        }

        protected override void OnViewDestroyed()
        {
            View.ContinueButton.Clicked -= OnContinueClicked;
            wallet.CoinsChanged -= OnCoinsChanged;
        }

        protected override void OnShowing()
        {
            ShowOffer();
        }

        private void OnContinueClicked()
        {
            if (wallet.TrySpend(offer.Cost))
            {
                Finish(true);
            }
        }

        private void OnCoinsChanged(int coins)
        {
            if (IsOpen)
            {
                ShowOffer();
            }
        }

        private void ShowOffer()
        {
            View.SetOffer(offer.Seconds, offer.Cost, wallet.Coins >= offer.Cost);
        }
    }
}
