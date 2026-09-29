using System.Threading;
using ColorBlockJam.Shared;
using Cysharp.Threading.Tasks;
using Framework.Core.SceneManagement;

namespace ColorBlockJam.Gameplay
{
    public interface ILevelFlow
    {
        /// <summary>Plays the same level again from its start state.</summary>
        void Restart();

        /// <summary>Plays the player's current level, which is the next one after a win.</summary>
        void PlayNext();

        void GoHome();
    }

    /// <summary>
    /// Leaves the level by loading a scene. Reloading the gameplay scene is what makes a restart clean:
    /// nothing from the previous attempt survives.
    /// </summary>
    public sealed class LevelFlow : ILevelFlow
    {
        private readonly ISceneLoader sceneLoader;
        private bool isLeaving;

        public LevelFlow(ISceneLoader sceneLoader)
        {
            this.sceneLoader = sceneLoader;
        }

        public void Restart() => Load(GameScenes.Gameplay);

        public void PlayNext() => Load(GameScenes.Gameplay);

        public void GoHome() => Load(GameScenes.Main);

        private void Load(string scene)
        {
            if (isLeaving)
            {
                return;
            }

            isLeaving = true;
            sceneLoader.LoadAsync(scene, CancellationToken.None).Forget();
        }
    }
}
