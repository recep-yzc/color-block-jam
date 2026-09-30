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
        [Tooltip("Kazanılan coin'i gösteren yazı.")]
        [SerializeField] private TMP_Text rewardLabel;
        [Tooltip("Sonraki seviyeyi açan buton.")]
        [SerializeField] private ActionButton nextButton;
        [Tooltip("Ana ekrana dönen buton.")]
        [SerializeField] private ActionButton homeButton;

        public ActionButton NextButton => nextButton;
        public ActionButton HomeButton => homeButton;

        public void SetReward(int coins)
        {
            rewardLabel.SetText("+{0}", coins);
        }
    }
}
