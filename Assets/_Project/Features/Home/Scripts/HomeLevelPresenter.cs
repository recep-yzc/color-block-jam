using System.Threading;
using ColorBlockJam.Core.SceneManagement;
using ColorBlockJam.Progression;
using ColorBlockJam.Shared.UI.Views;
using Cysharp.Threading.Tasks;

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
            View.SetLevel(progression.CurrentLevel);
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
            sceneLoader.LoadAsync(SceneNames.Gameplay, CancellationToken.None).Forget();
        }
    }
}
