using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Gameplay;
using ColorBlockJam.Level;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Obstacles
{
    public sealed class ObstacleIntros : ILevelIntro
    {
        private readonly ObstacleCatalog catalog;
        private readonly LevelData level;
        private readonly ISeenObstacles seen;
        private readonly IWindows windows;

        public ObstacleIntros(ObstacleCatalog catalog, LevelData level, ISeenObstacles seen, IWindows windows)
        {
            this.catalog = catalog;
            this.level = level;
            this.seen = seen;
            this.windows = windows;
        }

        public bool IsPending => Pending().Count > 0;

        public async UniTask PresentAsync(CancellationToken cancellationToken)
        {
            foreach (var obstacle in Pending())
            {
                if (!await windows.Get<ObstacleIntroPopupPresenter>().ShowAsync(obstacle, cancellationToken))
                {
                    return;
                }

                seen.MarkSeen(obstacle);
            }
        }

        private List<ObstacleDefinition> Pending()
        {
            return ObstacleRules.Pending(catalog.Obstacles, level, seen);
        }
    }
}
