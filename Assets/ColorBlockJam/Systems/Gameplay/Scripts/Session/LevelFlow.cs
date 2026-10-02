using System.Threading;
using ColorBlockJam.Core.SceneManagement;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelFlow : ILevelFlow
    {
        private readonly ISceneLoader sceneLoader;
        private readonly LevelRunner runner;
        private bool isLeaving;

        public LevelFlow(ISceneLoader sceneLoader, LevelRunner runner)
        {
            this.sceneLoader = sceneLoader;
            this.runner = runner;
        }

        public void Restart() => Reopen();

        public void PlayNext() => Reopen();

        public void GoHome()
        {
            if (isLeaving)
            {
                return;
            }

            isLeaving = true;
            sceneLoader.LoadAsync(GameScenes.Main, CancellationToken.None).Forget();
        }

        private void Reopen()
        {
            if (!isLeaving)
            {
                runner.Restart();
            }
        }
    }
}
