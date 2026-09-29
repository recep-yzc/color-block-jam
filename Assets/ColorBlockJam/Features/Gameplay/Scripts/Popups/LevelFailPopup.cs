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
        [SerializeField] private TMP_Text reasonLabel;
        [SerializeField] private string timeUpText = "Time's up!";
        [SerializeField] private string stuckText = "No moves left!";
        [SerializeField] private ActionButton restartButton;
        [SerializeField] private ActionButton homeButton;

        public ActionButton RestartButton => restartButton;
        public ActionButton HomeButton => homeButton;

        public void SetReason(LevelFailReason reason)
        {
            reasonLabel.text = reason == LevelFailReason.TimeUp ? timeUpText : stuckText;
        }
    }
}
