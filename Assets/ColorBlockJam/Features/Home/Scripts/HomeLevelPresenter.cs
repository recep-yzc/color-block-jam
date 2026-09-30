using System.Threading;
using ColorBlockJam.Progression;
using ColorBlockJam.Shared;
using Cysharp.Threading.Tasks;
using Framework.Core.SceneManagement;
using Framework.UI.Views;

namespace ColorBlockJam.Home
{
    public sealed class HomeLevelPresenter : ViewPresenter<HomeLevelView>
    {
        private readonly IProgressionService progression;
        private readonly ISceneLoader sceneLoader;

        public HomeLevelPresenter(HomeLevelView view, IProgressionService progression, ISceneLoader sceneLoader)
            : base(view)
        {
            this.progression = progression;
            this.sceneLoader = sceneLoader;
        }

        protected override void OnInitialize()
        {
            // Only the level the player is on is open; the path above it is still to come.
            View.Show(progression.CurrentLevel, lastUnlocked: progression.CurrentLevel);
            View.PlayButton.Clicked += OnPlayClicked;
        }

        protected override void OnDispose()
        {
            View.PlayButton.Clicked -= OnPlayClicked;
        }

        private void OnPlayClicked()
        {
            // One tap starts one load.
            View.PlayButton.Interactable = false;
            sceneLoader.LoadAsync(GameScenes.Gameplay, CancellationToken.None).Forget();
        }
    }
}
