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
        [Tooltip("Her kaybetme nedeni için gösterilen yazı ve ikon.")]
        [SerializeField] private FailReasonDisplay[] reasons = { };
        [Tooltip("Seviyeyi baştan başlatan buton.")]
        [SerializeField] private ActionButton restartButton;
        [Tooltip("Ana ekrana dönen buton.")]
        [SerializeField] private ActionButton homeButton;

        public ActionButton RestartButton => restartButton;
        public ActionButton HomeButton => homeButton;

        public void SetReason(LevelFailReason reason)
        {
            foreach (var display in reasons)
            {
                if (display.reason == reason)
                {
                    reasonLabel.text = display.text;
                    reasonIcon.sprite = display.icon;
                    return;
                }
            }

            reasonLabel.text = reason.ToString();
        }
    }
}
