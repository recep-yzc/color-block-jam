using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Framework.Core.SceneManagement
{
    public interface ISceneLoader
    {
        UniTask<PreloadedScene> PreloadAsync(string sceneName, IProgress<float> progress, CancellationToken cancellationToken);

        UniTask LoadAsync(string sceneName, CancellationToken cancellationToken);
    }
}
