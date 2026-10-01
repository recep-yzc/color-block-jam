using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace ColorBlockJam.Core.SceneManagement
{
    public sealed class SceneLoader : ISceneLoader
    {
        private const float LoadedProgress = 0.9f;

        public async UniTask<PreloadedScene> PreloadAsync(string sceneName, IProgress<float> progress, CancellationToken cancellationToken)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            operation.allowSceneActivation = false;

            while (operation.progress < LoadedProgress)
            {
                progress?.Report(operation.progress / LoadedProgress);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            progress?.Report(1f);
            return new PreloadedScene(operation);
        }

        public async UniTask LoadAsync(string sceneName, CancellationToken cancellationToken)
        {
            var scene = await PreloadAsync(sceneName, null, cancellationToken);
            scene.Activate();
        }
    }
}
