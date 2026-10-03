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
        [Tooltip("Seviye yazısının biçimi. {0} seviye numarasının ya da test seviyesinde 'Test' yazısının yeridir.")]
        [SerializeField] private string levelFormat = "<size=40>Level</size><br>{0}";
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
        [Tooltip("Her zorluk için rozette yazan ad ve rozetin rengi. Listede olmayan bir zorluk adıyla gösterilir.")]
        [SerializeField] private DifficultyBadge[] difficultyBadges = { };
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

        protected override void OnDestroy()
        {
            blink.TryCancel();
            base.OnDestroy();
        }

        public void SetLevel(int level)
        {
            levelLabel.SetText(levelFormat, level);
        }

        public void SetTestLevel()
        {
            levelLabel.text = levelFormat.Replace("{0}", "Test");
        }

        public void SetDifficulty(LevelDifficulty difficulty)
        {
            foreach (var badge in difficultyBadges)
            {
                if (badge.difficulty == difficulty)
                {
                    difficultyLabel.text = badge.label;
                    difficultyLabel.color = badge.color;
                    return;
                }
            }

            difficultyLabel.text = difficulty.ToString();
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
                        .BindToColor(timerLabel);
                }

                return;
            }

            blink.TryCancel();
            timerLabel.color = isTimerFrozen ? timerFrozenColor : timerColor;
        }
    }
}
