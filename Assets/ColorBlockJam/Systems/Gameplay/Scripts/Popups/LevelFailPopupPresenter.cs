using System.Threading;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelFailPopupPresenter : WindowPresenter<LevelFailPopup>
    {
        private LevelFailReason reason;

        public UniTask<LevelFailChoice> ShowAsync(LevelFailReason failReason, CancellationToken cancellationToken = default)
        {
            reason = failReason;
            return OpenAsync(LevelFailChoice.Retry, cancellationToken);
        }

        protected override void OnViewCreated()
        {
            View.RestartButton.Clicked += OnRetryClicked;
            View.HomeButton.Clicked += OnHomeClicked;
        }

        protected override void OnViewDestroyed()
        {
            View.RestartButton.Clicked -= OnRetryClicked;
            View.HomeButton.Clicked -= OnHomeClicked;
        }

        protected override void OnShowing()
        {
            View.SetReason(reason);
        }

        private void OnRetryClicked()
        {
            Finish(LevelFailChoice.Retry);
        }

        private void OnHomeClicked()
        {
            Finish(LevelFailChoice.Home);
        }
    }
}
