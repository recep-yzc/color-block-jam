using ColorBlockJam.Level;
using ColorBlockJam.UI;
using ColorBlockJam.UI.Buttons;
using ColorBlockJam.UI.Views;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class GameplayHudView : UIView
    {
        [Tooltip("Seviye numarasını gösteren yazı.")]
        [SerializeField] private TMP_Text levelLabel;
        [Tooltip("Seviyenin zorluğunu gösteren yazı.")]
        [SerializeField] private TMP_Text difficultyLabel;
        [Tooltip("Kalan süreyi dakika:saniye olarak gösteren yazı.")]
        [SerializeField] private TMP_Text timerLabel;
        [Tooltip("Süre azalınca sayacın kırmızıdan beyaza ya da beyazdan kırmızıya geçme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float timerBlinkDuration = 0.5f;
        [Tooltip("Sayacın normal rengi.")]
        [SerializeField] private Color timerColor = Color.white;
        [Tooltip("Süre azalınca sayacın yanıp söndüğü renk.")]
        [SerializeField] private Color timerWarningColor = new(1f, 0.35f, 0.3f);
        [Tooltip("Freeze süreyi tutarken sayacın rengi.")]
        [SerializeField] private Color timerFrozenColor = new(0.6f, 0.88f, 1f);
        [Tooltip("Kolay seviyelerin zorluk rozeti rengi.")]
        [SerializeField] private Color easyColor = new(0.55f, 0.9f, 0.45f);
        [Tooltip("Orta seviyelerin zorluk rozeti rengi.")]
        [SerializeField] private Color mediumColor = new(1f, 0.8f, 0.3f);
        [Tooltip("Zor seviyelerin zorluk rozeti rengi.")]
        [SerializeField] private Color hardColor = new(1f, 0.5f, 0.35f);
        [Tooltip("Süper zor seviyelerin zorluk rozeti rengi.")]
        [SerializeField] private Color superHardColor = new(0.85f, 0.45f, 1f);
        [Tooltip("Seviyeyi baştan başlatan buton.")]
        [SerializeField] private ActionButton restartButton;
        [Tooltip("Oyunu durdurup ayarlar popup'ını açan buton.")]
        [SerializeField] private ActionButton pauseButton;
        [Tooltip("Seviyeyi bulunduğu yerden çözücüye oynatan buton.")]
        [SerializeField] private ActionButton autoPlayButton;

        private MotionHandle blink;
        private bool isTimerWarning;
        private bool isTimerFrozen;

        public ActionButton RestartButton => restartButton;
        public ActionButton PauseButton => pauseButton;
        public ActionButton AutoPlayButton => autoPlayButton;

        public void SetLevel(int level)
        {
            levelLabel.SetText("<size=40>Level</size><br>{0}", level);
        }

        public void SetTestLevel()
        {
            levelLabel.text = "<size=40>Level</size><br>Test";
        }

        public void SetDifficulty(LevelDifficulty difficulty)
        {
            (difficultyLabel.text, difficultyLabel.color) = difficulty switch
            {
                LevelDifficulty.Easy => ("Easy", easyColor),
                LevelDifficulty.Medium => ("Medium", mediumColor),
                LevelDifficulty.Hard => ("Hard", hardColor),
                _ => ("Super Hard", superHardColor)
            };
        }

        public void SetTime(int seconds, bool isWarning)
        {
            timerLabel.SetText("{0}:{1:00}", seconds / 60, seconds % 60);
            isTimerWarning = isWarning;
            ShowTimerColor();
        }

        public void SetTimerFrozen(bool isFrozen)
        {
            isTimerFrozen = isFrozen;
            ShowTimerColor();
        }

        private void ShowTimerColor()
        {
            if (isTimerWarning && !isTimerFrozen)
            {
                if (!blink.IsActive())
                {
                    blink = LMotion.Create(timerWarningColor, timerColor, timerBlinkDuration)
                        .WithLoops(-1, LoopType.Yoyo)
                        .WithScheduler(UIMotion.Scheduler)
                        .BindToColor(timerLabel)
                        .AddTo(this);
                }

                return;
            }

            blink.TryCancel();
            timerLabel.color = isTimerFrozen ? timerFrozenColor : timerColor;
        }
    }
}
