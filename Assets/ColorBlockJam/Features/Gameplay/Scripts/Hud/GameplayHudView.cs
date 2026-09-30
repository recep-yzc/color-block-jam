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
    /// The countdown turns icy while the freeze booster holds it.
    /// </summary>
    public sealed class GameplayHudView : UIView
    {
        [Tooltip("Seviye numarasını gösteren yazı.")]
        [SerializeField] private TMP_Text levelLabel;
        [Tooltip("Seviyenin zorluğunu gösteren yazı.")]
        [SerializeField] private TMP_Text difficultyLabel;
        [Tooltip("Kalan süreyi dakika:saniye olarak gösteren yazı.")]
        [SerializeField] private TMP_Text timerLabel;
        [Tooltip("Süre uyarı aralığındayken her saniye zıplatılan obje.")]
        [SerializeField] private RectTransform timerPulseTarget;
        [Tooltip("Uyarı zıplamasının büyüklüğü, ölçeğe oranla.")]
        [SerializeField] private float timerPulseStrength = 0.18f;
        [Tooltip("Bir uyarı zıplamasının süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float timerPulseDuration = 0.3f;
        [Tooltip("Sayacın normal rengi.")]
        [SerializeField] private Color timerColor = Color.white;
        [Tooltip("Süre azalınca sayacın rengi.")]
        [SerializeField] private Color timerWarningColor = new(1f, 0.35f, 0.3f);
        [Tooltip("Freeze süreyi tutarken sayacın rengi.")]
        [SerializeField] private Color timerFrozenColor = new(0.6f, 0.88f, 1f);
        [Tooltip("Duraklatma popup'ını açan buton.")]
        [SerializeField] private ActionButton pauseButton;
        [Tooltip("Seviyeyi bulunduğu yerden çözücüye oynatan buton.")]
        [SerializeField] private ActionButton autoPlayButton;

        private MotionHandle pulse;
        private bool isTimerWarning;
        private bool isTimerFrozen;

        public ActionButton PauseButton => pauseButton;
        public ActionButton AutoPlayButton => autoPlayButton;

        public void SetLevel(int level)
        {
            levelLabel.SetText("Level {0}", level);
        }

        /// <summary>Shown instead of the level number for a level played from the level editor.</summary>
        public void SetTestLevel()
        {
            levelLabel.text = "Test Level";
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
            isTimerWarning = isWarning;
            ShowTimerColor();

            if (isWarning && timerPulseTarget != null)
            {
                pulse.TryComplete();
                pulse = LMotion.Punch.Create(Vector3.one, Vector3.one * timerPulseStrength, timerPulseDuration)
                    .WithScheduler(UIMotion.Scheduler)
                    .BindToLocalScale(timerPulseTarget)
                    .AddTo(this);
            }
        }

        /// <summary>Shows the timer as frozen while the freeze booster holds it.</summary>
        public void SetTimerFrozen(bool isFrozen)
        {
            isTimerFrozen = isFrozen;
            ShowTimerColor();
        }

        private void ShowTimerColor()
        {
            timerLabel.color = isTimerFrozen ? timerFrozenColor : isTimerWarning ? timerWarningColor : timerColor;
        }
    }
}
