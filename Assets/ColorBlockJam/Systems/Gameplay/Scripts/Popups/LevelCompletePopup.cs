using ColorBlockJam.UI.Buttons;
using ColorBlockJam.UI.Windows;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelCompletePopup : WindowView
    {
        [Tooltip("Kazanılan coin'i gösteren yazı.")]
        [SerializeField] private TMP_Text rewardLabel;
        [Tooltip("Sonraki seviyeyi açan buton.")]
        [SerializeField] private ActionButton nextButton;

        public ActionButton NextButton => nextButton;

        public void SetReward(int coins)
        {
            rewardLabel.SetText("{0}", coins);
        }
    }
}
