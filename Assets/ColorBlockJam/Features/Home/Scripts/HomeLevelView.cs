using ColorBlockJam.Shared.UI.Buttons;
using ColorBlockJam.Shared.UI.Views;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// The level button of the home page.
    /// </summary>
    public sealed class HomeLevelView : UIView
    {
        [SerializeField] private ActionButton playButton;
        [SerializeField] private TMP_Text levelLabel;

        public ActionButton PlayButton => playButton;

        public void SetLevel(int level)
        {
            levelLabel.SetText("Level {0}", level);
        }
    }
}
