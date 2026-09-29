using Framework.UI.Buttons;
using Framework.UI.Views;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// The level button and the level path of the home page.
    /// </summary>
    public sealed class HomeLevelView : UIView
    {
        [SerializeField] private ActionButton playButton;
        [SerializeField] private TMP_Text levelLabel;
        [Tooltip("Tiles on the path, from the current level upwards.")]
        [SerializeField] private LevelPathNodeView[] pathNodes;

        public ActionButton PlayButton => playButton;

        public void SetLevel(int level)
        {
            levelLabel.SetText("Level {0}", level);

            for (var i = 0; i < pathNodes.Length; i++)
            {
                pathNodes[i].SetLevel(level + i);
            }
        }
    }
}
