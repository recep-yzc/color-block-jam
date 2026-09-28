using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Core.SceneManagement
{
    public interface ISceneLoader
    {
        /// <summary>
        /// Loads the scene in the background without activating it.
        /// Call <see cref="PreloadedScene.Activate"/> to switch to the scene.
        /// </summary>
        UniTask<PreloadedScene> PreloadAsync(string sceneName, IProgress<float> progress, CancellationToken cancellationToken);

        /// <summary>
        /// Loads the scene in the background and switches to it as soon as loading is complete.
        /// </summary>
        UniTask LoadAsync(string sceneName, CancellationToken cancellationToken);
    }
}
