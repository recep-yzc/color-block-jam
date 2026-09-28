using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Core.SceneManagement;
using ColorBlockJam.Core.Startup;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Boot
{
    /// <summary>
    /// Runs the startup tasks, preloads the main scene and switches to it
    /// once the loading bar is full.
    /// </summary>
    public sealed class SplashFlow : IAsyncStartable
    {
        private readonly IReadOnlyList<IStartupTask> startupTasks;
        private readonly ISceneLoader sceneLoader;
        private readonly SplashConfig config;
        private readonly LoadingBarView loadingBar;

        private float loadedProgress;

        public SplashFlow(
            IReadOnlyList<IStartupTask> startupTasks,
            ISceneLoader sceneLoader,
            SplashConfig config,
            LoadingBarView loadingBar)
        {
            this.startupTasks = startupTasks;
            this.sceneLoader = sceneLoader;
            this.config = config;
            this.loadingBar = loadingBar;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            loadingBar.SetProgress(0f);
            var barAnimation = AnimateLoadingBarAsync(cancellation);

            await RunStartupTasksAsync(cancellation);
            var mainScene = await sceneLoader.PreloadAsync(
                SceneNames.Main,
                Progress.Create<float>(ReportSceneProgress),
                cancellation);

            await barAnimation;
            mainScene.Activate();
        }

        private async UniTask RunStartupTasksAsync(CancellationToken cancellation)
        {
            for (var i = 0; i < startupTasks.Count; i++)
            {
                await startupTasks[i].RunAsync(cancellation);
                loadedProgress = config.StartupTasksShare * (i + 1) / startupTasks.Count;
            }
        }

        private void ReportSceneProgress(float sceneProgress)
        {
            loadedProgress = config.StartupTasksShare + (1f - config.StartupTasksShare) * sceneProgress;
        }

        private async UniTask AnimateLoadingBarAsync(CancellationToken cancellation)
        {
            var startTime = Time.realtimeSinceStartup;
            var shownProgress = 0f;

            while (shownProgress < 1f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation);

                // The bar never runs ahead of real loading or of the minimum splash duration.
                var elapsedShare = config.MinimumDuration > 0f
                    ? (Time.realtimeSinceStartup - startTime) / config.MinimumDuration
                    : 1f;
                var targetProgress = Mathf.Min(loadedProgress, elapsedShare);

                shownProgress = Mathf.MoveTowards(shownProgress, targetProgress, config.BarFillSpeed * Time.unscaledDeltaTime);
                loadingBar.SetProgress(shownProgress);
            }
        }
    }
}
