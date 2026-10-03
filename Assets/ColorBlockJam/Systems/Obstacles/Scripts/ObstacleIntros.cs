using System.Threading;
using ColorBlockJam.Gameplay;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Obstacles
{
    public sealed class ObstacleIntros : ILevelIntro
    {
        private readonly ObstacleCatalog catalog;
        private readonly LevelSession session;
        private readonly ISeenObstacles seen;
        private readonly IWindows windows;

        public ObstacleIntros(ObstacleCatalog catalog, LevelSession session, ISeenObstacles seen, IWindows windows)
        {
            this.catalog = catalog;
            this.session = session;
            this.seen = seen;
            this.windows = windows;
        }

        public async UniTask PresentAsync(CancellationToken cancellationToken)
        {
            foreach (var obstacle in ObstacleRules.Pending(catalog.Obstacles, session.Level, seen))
            {
                if (!await windows.Get<ObstacleIntroPopupPresenter>().ShowAsync(obstacle, cancellationToken))
                {
                    return;
                }

                seen.MarkSeen(obstacle);
            }
        }
    }
}
