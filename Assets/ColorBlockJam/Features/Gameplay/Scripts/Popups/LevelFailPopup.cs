using Framework.UI.Buttons;
using Framework.UI.Popups;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Shown when the level is lost, with the reason. The only ways on are a restart or home.
    /// </summary>
    public sealed class LevelFailPopup : Popup
    {
        [Tooltip("Seviyenin neden kaybedildiğini gösteren yazı.")]
        [SerializeField] private TMP_Text reasonLabel;
        [Tooltip("Süre bitince gösterilen yazı.")]
        [SerializeField] private string timeUpText = "Time's up!";
        [Tooltip("Yapılacak hamle kalmayınca gösterilen yazı.")]
        [SerializeField] private string stuckText = "No moves left!";
        [Tooltip("Seviyeyi baştan başlatan buton.")]
        [SerializeField] private ActionButton restartButton;
        [Tooltip("Ana ekrana dönen buton.")]
        [SerializeField] private ActionButton homeButton;

        public ActionButton RestartButton => restartButton;
        public ActionButton HomeButton => homeButton;

        public void SetReason(LevelFailReason reason)
        {
            reasonLabel.text = reason == LevelFailReason.TimeUp ? timeUpText : stuckText;
        }
    }
}
