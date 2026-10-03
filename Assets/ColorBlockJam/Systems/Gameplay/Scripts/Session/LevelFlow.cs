using System.Threading;
using ColorBlockJam.Core.SceneManagement;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelFlow : ILevelFlow
    {
        private readonly ISceneLoader sceneLoader;
        private readonly LevelRunner runner;

        public LevelFlow(ISceneLoader sceneLoader, LevelRunner runner)
        {
            this.sceneLoader = sceneLoader;
            this.runner = runner;
        }

        public bool IsLeaving { get; private set; }

        public void Restart() => Reopen();

        public void PlayNext() => Reopen();

        public void GoHome()
        {
            if (IsLeaving)
            {
                return;
            }

            IsLeaving = true;
            sceneLoader.LoadAsync(GameScenes.Main, CancellationToken.None).Forget();
        }

        private void Reopen()
        {
            if (!IsLeaving)
            {
                runner.Restart();
            }
        }
    }
}
