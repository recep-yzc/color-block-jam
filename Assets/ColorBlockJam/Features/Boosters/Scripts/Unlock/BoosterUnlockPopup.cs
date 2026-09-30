using Framework.UI.Buttons;
using Framework.UI.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterUnlockPopup : Popup
    {
        [Tooltip("Açılan booster'ın ikonu.")]
        [SerializeField] private Image icon;
        [Tooltip("Açılan booster'ın adı.")]
        [SerializeField] private TMP_Text nameLabel;
        [Tooltip("Açılan booster'ın ne yaptığını anlatan yazı.")]
        [SerializeField] private TMP_Text descriptionLabel;
        [Tooltip("Booster'ı alıp popup'ı kapatan buton. Booster bu butona basılınca açılır.")]
        [SerializeField] private ActionButton claimButton;

        public ActionButton ClaimButton => claimButton;

        public void SetBooster(BoosterDefinition booster)
        {
            icon.sprite = booster.Icon;
            nameLabel.text = booster.DisplayName;
            descriptionLabel.text = booster.Description;
        }
    }
}
