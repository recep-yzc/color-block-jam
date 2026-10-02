using ColorBlockJam.UI.Views;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterBarView : UIView
    {
        [Tooltip("Katalogdaki her booster için bara eklenen buton.")]
        [SerializeField] private BoosterButtonView buttonPrefab;
        [Tooltip("Butonların dizildiği obje. Layout group'u onları yan yana koyar, gizli butonlar yer kaplamaz.")]
        [SerializeField] private RectTransform buttonParent;
        [Tooltip("Hedefli bir booster kalkıkken oyuncuya ne yapacağını söyleyen yazı.")]
        [SerializeField] private TMP_Text aimHint;

        public BoosterButtonView AddButton(Sprite icon)
        {
            var button = Instantiate(buttonPrefab, buttonParent);
            button.SetIcon(icon);
            return button;
        }

        public void RemoveButton(BoosterButtonView button)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }

        public void ShowAimHint(string hint)
        {
            aimHint.text = hint;
            aimHint.gameObject.SetActive(true);
        }

        public void HideAimHint()
        {
            if (aimHint != null)
            {
                aimHint.gameObject.SetActive(false);
            }
        }
    }
}
