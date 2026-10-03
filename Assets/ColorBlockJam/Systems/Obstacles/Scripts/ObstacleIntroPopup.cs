using ColorBlockJam.UI.Buttons;
using ColorBlockJam.UI.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Obstacles
{
    public sealed class ObstacleIntroPopup : WindowView
    {
        [Tooltip("Tanıtılan engelin ikonu. Engelin ikonu yoksa gizlenir.")]
        [SerializeField] private Image icon;
        [Tooltip("Tanıtılan engelin adı.")]
        [SerializeField] private TMP_Text nameLabel;
        [Tooltip("Engelin nasıl çalıştığını anlatan yazı.")]
        [SerializeField] private TMP_Text descriptionLabel;
        [Tooltip("Popup'ı kapatıp seviyeye dönen buton.")]
        [SerializeField] private ActionButton continueButton;

        public ActionButton ContinueButton => continueButton;

        public void SetObstacle(ObstacleDefinition obstacle)
        {
            icon.sprite = obstacle.Icon;
            icon.enabled = obstacle.Icon != null;
            nameLabel.text = obstacle.DisplayName;
            descriptionLabel.text = obstacle.Description;
        }
    }
}
