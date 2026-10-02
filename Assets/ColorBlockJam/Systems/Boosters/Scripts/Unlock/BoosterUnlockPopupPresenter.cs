using System.Threading;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterUnlockPopupPresenter : WindowPresenter<BoosterUnlockPopup>
    {
        private BoosterDefinition booster;

        public UniTask<bool> ShowAsync(BoosterDefinition unlocked, CancellationToken cancellationToken = default)
        {
            booster = unlocked;
            return OpenAsync(false, cancellationToken);
        }

        protected override void OnViewCreated()
        {
            View.ClaimButton.Clicked += OnClaimClicked;
        }

        protected override void OnViewDestroyed()
        {
            View.ClaimButton.Clicked -= OnClaimClicked;
        }

        protected override void OnShowing()
        {
            View.SetBooster(booster);
        }

        private void OnClaimClicked()
        {
            Finish(true);
        }
    }
}
