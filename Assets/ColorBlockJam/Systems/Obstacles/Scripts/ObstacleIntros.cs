using System;
using System.Threading;
using ColorBlockJam.Gameplay;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace ColorBlockJam.Obstacles
{
    public sealed class ObstacleIntros : IStartable, IDisposable
    {
        private readonly ObstacleCatalog catalog;
        private readonly ILevelProvider levels;
        private readonly IObstacleIntroductions introductions;
        private readonly IWindows windows;
        private readonly CancellationTokenSource lifetime = new();

        public ObstacleIntros(ObstacleCatalog catalog, ILevelProvider levels, IObstacleIntroductions introductions, IWindows windows)
        {
            this.catalog = catalog;
            this.levels = levels;
            this.introductions = introductions;
            this.windows = windows;
        }

        public void Start()
        {
            if (!levels.IsEditorTest)
            {
                PresentAsync().Forget();
            }
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }

        private async UniTaskVoid PresentAsync()
        {
            var token = lifetime.Token;
            foreach (var obstacle in ObstacleRules.Pending(catalog.Obstacles, levels.Load(), introductions))
            {
                var (isCanceled, _) = await windows.Get<ObstacleIntroPopupPresenter>().ShowAsync(obstacle, token).SuppressCancellationThrow();
                if (isCanceled)
                {
                    return;
                }

                introductions.MarkIntroduced(obstacle);
            }
        }
    }
}
