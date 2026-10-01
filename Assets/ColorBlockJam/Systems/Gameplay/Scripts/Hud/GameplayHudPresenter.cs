using ColorBlockJam.UI.Views;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class GameplayHudPresenter : ViewPresenter<GameplayHudView>, ITickable
    {
        private readonly LevelSession session;
        private readonly PauseRequester pause;
        private readonly ILevelFlow flow;
        private readonly GameplayConfig config;
        private int shownSeconds = -1;
        private bool shownFrozen;

        public GameplayHudPresenter(GameplayHudView view, LevelSession session, PauseRequester pause, ILevelFlow flow,
            GameplayConfig config)
            : base(view)
        {
            this.session = session;
            this.pause = pause;
            this.flow = flow;
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
            View.RestartButton.Clicked += flow.Restart;
            View.PauseButton.Clicked += OnPauseClicked;
            View.AutoPlayButton.Clicked += OnAutoPlayClicked;
            session.StateChanged += OnStateChanged;
            Tick();
        }

        protected override void OnDispose()
        {
            View.RestartButton.Clicked -= flow.Restart;
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
            View.RestartButton.Interactable = isPlaying;
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
