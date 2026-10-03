using ColorBlockJam.UI.Buttons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.UI.Windows
{
    public sealed class ShowcasePopup : WindowView
    {
        [Tooltip("Tanıtılan şeyin ikonu. İkon verilmezse gizlenir.")]
        [SerializeField] private Image icon;
        [Tooltip("Tanıtılan şeyin adı.")]
        [SerializeField] private TMP_Text nameLabel;
        [Tooltip("Tanıtılan şeyin ne yaptığını anlatan yazı.")]
        [SerializeField] private TMP_Text descriptionLabel;
        [Tooltip("Popup'ı kapatan onay butonu. Yazısı her prefab varyantında ayrıdır.")]
        [SerializeField] private ActionButton confirmButton;

        public ActionButton ConfirmButton => confirmButton;

        public void Show(Sprite iconSprite, string displayName, string description)
        {
            icon.sprite = iconSprite;
            icon.enabled = iconSprite != null;
            nameLabel.text = displayName;
            descriptionLabel.text = description;
        }
    }
}
