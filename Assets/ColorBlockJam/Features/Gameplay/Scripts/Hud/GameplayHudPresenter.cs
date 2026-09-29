using Cysharp.Threading.Tasks;
using Framework.UI.Popups;
using Framework.UI.Views;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class GameplayHudPresenter : ViewPresenter<GameplayHudView>, ITickable
    {
        private readonly LevelSession session;
        private readonly IPopupService popups;
        private readonly GameplayConfig config;
        private int shownSeconds = -1;

        public GameplayHudPresenter(GameplayHudView view, LevelSession session, IPopupService popups, GameplayConfig config)
            : base(view)
        {
            this.session = session;
            this.popups = popups;
            this.config = config;
        }

        protected override void OnInitialize()
        {
            View.SetLevel(session.LevelNumber);
            View.SetDifficulty(session.Level.difficulty);
            View.PauseButton.Clicked += OnPauseClicked;
            View.AutoPlayButton.Clicked += OnAutoPlayClicked;
            session.StateChanged += OnStateChanged;
            Tick();
        }

        protected override void OnDispose()
        {
            View.PauseButton.Clicked -= OnPauseClicked;
            View.AutoPlayButton.Clicked -= OnAutoPlayClicked;
            session.StateChanged -= OnStateChanged;
        }

        public void Tick()
        {
            // The label only changes once a second, so it is rebuilt only then.
            var seconds = Mathf.CeilToInt(session.Timer.Remaining);
            if (seconds == shownSeconds)
            {
                return;
            }

            shownSeconds = seconds;
            View.SetTime(seconds, seconds <= config.TimerWarningSeconds && session.State == LevelState.Playing);
        }

        private void OnStateChanged()
        {
            var isPlaying = session.State == LevelState.Playing;
            View.PauseButton.Interactable = isPlaying;
            View.AutoPlayButton.Interactable = isPlaying;
        }

        private void OnPauseClicked()
        {
            popups.ShowAsync<PausePopup>().Forget();
        }

        private void OnAutoPlayClicked()
        {
            session.StartAutoPlay();
        }
    }
}
