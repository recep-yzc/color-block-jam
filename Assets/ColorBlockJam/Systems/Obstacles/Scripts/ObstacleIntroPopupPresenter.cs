using System.Threading;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Obstacles
{
    public sealed class ObstacleIntroPopupPresenter : ShowcasePopupPresenter
    {
        public UniTask<bool> ShowAsync(ObstacleDefinition obstacle, CancellationToken cancellationToken = default)
        {
            return ShowAsync(obstacle.Icon, obstacle.DisplayName, obstacle.Description, cancellationToken);
        }
    }
}
