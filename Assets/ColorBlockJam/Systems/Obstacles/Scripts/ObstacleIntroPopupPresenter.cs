using System.Threading;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Obstacles
{
    public sealed class ObstacleIntroPopupPresenter : WindowPresenter<ObstacleIntroPopup>
    {
        private ObstacleDefinition obstacle;

        public UniTask<bool> ShowAsync(ObstacleDefinition introduced, CancellationToken cancellationToken = default)
        {
            obstacle = introduced;
            return OpenAsync(false, cancellationToken);
        }

        protected override void OnViewCreated()
        {
            View.ContinueButton.Clicked += OnContinueClicked;
        }

        protected override void OnViewDestroyed()
        {
            View.ContinueButton.Clicked -= OnContinueClicked;
        }

        protected override void OnShowing()
        {
            View.SetObstacle(obstacle);
        }

        private void OnContinueClicked()
        {
            Finish(true);
        }
    }
}
