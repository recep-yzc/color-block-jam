using Framework.UI.Buttons;
using Framework.UI.Popups;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Shown when every block has left the board, with the coins won.
    /// </summary>
    public sealed class LevelCompletePopup : Popup
    {
        [SerializeField] private TMP_Text rewardLabel;
        [SerializeField] private ActionButton nextButton;
        [SerializeField] private ActionButton homeButton;

        public ActionButton NextButton => nextButton;
        public ActionButton HomeButton => homeButton;

        public void SetReward(int coins)
        {
            rewardLabel.SetText("+{0}", coins);
        }
    }
}
