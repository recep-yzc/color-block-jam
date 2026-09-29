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
        [SerializeField] private TMP_Text numberLabel;
        [Tooltip("Shown only on tiles of levels the player has not reached.")]
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private Graphic background;
        [Tooltip("Multiplies the background color of locked tiles.")]
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
