using ColorBlockJam.Level;
using Framework.UI;
using Framework.UI.Buttons;
using Framework.UI.Views;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The top bar of the level: level number, difficulty, the countdown, pause and auto play.
    /// The booster buttons below the board are placeholder buttons and need no code.
    /// </summary>
    public sealed class GameplayHudView : UIView
    {
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text difficultyLabel;
        [SerializeField] private TMP_Text timerLabel;
        [Tooltip("Scaled with a punch each second while the timer is in its warning range.")]
        [SerializeField] private RectTransform timerPulseTarget;
        [SerializeField] private Color timerColor = Color.white;
        [SerializeField] private Color timerWarningColor = new(1f, 0.35f, 0.3f);
        [SerializeField] private ActionButton pauseButton;
        [SerializeField] private ActionButton autoPlayButton;

        private MotionHandle pulse;

        public ActionButton PauseButton => pauseButton;
        public ActionButton AutoPlayButton => autoPlayButton;

        public void SetLevel(int level)
        {
            levelLabel.SetText("Level {0}", level);
        }

        public void SetDifficulty(LevelDifficulty difficulty)
        {
            difficultyLabel.text = difficulty switch
            {
                LevelDifficulty.Easy => "Easy",
                LevelDifficulty.Medium => "Medium",
                _ => "Hard"
            };
        }

        public void SetTime(int seconds, bool isWarning)
        {
            timerLabel.SetText("{0}:{1:00}", seconds / 60, seconds % 60);
            timerLabel.color = isWarning ? timerWarningColor : timerColor;

            if (isWarning && timerPulseTarget != null)
            {
                pulse.TryComplete();
                pulse = LMotion.Punch.Create(Vector3.one, Vector3.one * 0.18f, 0.3f)
                    .WithScheduler(UIMotion.Scheduler)
                    .BindToLocalScale(timerPulseTarget)
                    .AddTo(this);
            }
        }
    }
}
