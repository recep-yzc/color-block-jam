using Framework.UI.Views;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class GameplayHudPresenter : ViewPresenter<GameplayHudView>, ITickable
    {
        private readonly LevelSession session;
        private readonly PauseRequester pause;
        private readonly GameplayConfig config;
        private int shownSeconds = -1;
        private bool shownFrozen;

        public GameplayHudPresenter(GameplayHudView view, LevelSession session, PauseRequester pause, GameplayConfig config)
            : base(view)
        {
            this.session = session;
            this.pause = pause;
            this.config = config;
        }

        protected override void OnInitialize()
        {
            if (session.IsEditorTest)
            {
                View.SetTestLevel();
            }
            else
            {
                View.SetLevel(session.LevelNumber);
            }

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
            var isFrozen = session.Timer.IsFrozen;
            if (isFrozen != shownFrozen)
            {
                shownFrozen = isFrozen;
                View.SetTimerFrozen(isFrozen);
            }

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
            pause.Request();
        }

        private void OnAutoPlayClicked()
        {
            session.StartAutoPlay();
        }
    }
}
