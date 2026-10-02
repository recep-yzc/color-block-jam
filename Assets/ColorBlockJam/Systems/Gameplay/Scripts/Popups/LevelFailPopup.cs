using ColorBlockJam.UI.Buttons;
using ColorBlockJam.UI.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelFailPopup : WindowView
    {
        [Tooltip("Seviyenin neden kaybedildiğini gösteren yazı.")]
        [SerializeField] private TMP_Text reasonLabel;
        [Tooltip("Seviyenin neden kaybedildiğini gösteren ikon.")]
        [SerializeField] private Image reasonIcon;
        [Tooltip("Süre bitince gösterilen yazı.")]
        [SerializeField] private string timeUpText = "Time's up!";
        [Tooltip("Süre bitince gösterilen ikon.")]
        [SerializeField] private Sprite timeUpIcon;
        [Tooltip("Yapılacak hamle kalmayınca gösterilen yazı.")]
        [SerializeField] private string stuckText = "No moves left!";
        [Tooltip("Yapılacak hamle kalmayınca gösterilen ikon.")]
        [SerializeField] private Sprite stuckIcon;
        [Tooltip("Seviyeyi baştan başlatan buton.")]
        [SerializeField] private ActionButton restartButton;
        [Tooltip("Ana ekrana dönen buton.")]
        [SerializeField] private ActionButton homeButton;

        public ActionButton RestartButton => restartButton;
        public ActionButton HomeButton => homeButton;

        public void SetReason(LevelFailReason reason)
        {
            var isTimeUp = reason == LevelFailReason.TimeUp;
            reasonLabel.text = isTimeUp ? timeUpText : stuckText;
            reasonIcon.sprite = isTimeUp ? timeUpIcon : stuckIcon;
        }
    }
}
