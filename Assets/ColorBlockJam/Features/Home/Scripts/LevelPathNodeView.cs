using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// One level tile on the home page path. The current level's tile keeps its colors;
    /// tiles of later levels show a lock and a faded background.
    /// </summary>
    public sealed class LevelPathNodeView : MonoBehaviour
    {
        [Tooltip("Karonun seviye numarası.")]
        [SerializeField] private TMP_Text numberLabel;
        [Tooltip("Sadece oyuncunun henüz ulaşmadığı seviyelerin karolarında görünür.")]
        [SerializeField] private GameObject lockIcon;
        [Tooltip("Kilitli karolarda renklendirilen arka plan.")]
        [SerializeField] private Graphic background;
        [Tooltip("Kilitli karoların arka plan rengiyle çarpılan renk.")]
        [SerializeField] private Color lockedTint = new(0.6f, 0.6f, 0.75f, 1f);

        private Color unlockedColor;
        private bool hasUnlockedColor;

        public void Show(int level, bool isLocked)
        {
            numberLabel.SetText("{0}", level);
            lockIcon.SetActive(isLocked);

            if (!hasUnlockedColor)
            {
                unlockedColor = background.color;
                hasUnlockedColor = true;
            }

            background.color = isLocked ? unlockedColor * lockedTint : unlockedColor;
        }
    }
}
