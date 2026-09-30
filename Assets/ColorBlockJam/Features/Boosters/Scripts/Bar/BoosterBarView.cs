using Framework.UI.Views;
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

        public void ShowAimHint(string hint)
        {
            aimHint.text = hint;
            aimHint.gameObject.SetActive(true);
        }

        public void HideAimHint()
        {
            aimHint.gameObject.SetActive(false);
        }
    }
}
