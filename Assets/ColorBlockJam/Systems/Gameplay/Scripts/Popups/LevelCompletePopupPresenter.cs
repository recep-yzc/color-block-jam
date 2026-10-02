using System.Threading;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelCompletePopupPresenter : WindowPresenter<LevelCompletePopup>
    {
        private int reward;

        public UniTask<bool> ShowAsync(int coins, CancellationToken cancellationToken = default)
        {
            reward = coins;
            return OpenAsync(false, cancellationToken);
        }

        protected override void OnViewCreated()
        {
            View.NextButton.Clicked += OnNextClicked;
        }

        protected override void OnViewDestroyed()
        {
            View.NextButton.Clicked -= OnNextClicked;
        }

        protected override void OnShowing()
        {
            View.SetReward(reward);
        }

        private void OnNextClicked()
        {
            Finish(true);
        }
    }
}
