using ColorBlockJam.Shared.UI.Buttons;
using ColorBlockJam.Shared.UI.Views;
using UnityEngine;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// The top bar of the home page.
    /// </summary>
    public sealed class HomeHudView : UIView
    {
        [SerializeField] private ActionButton settingsButton;

        public ActionButton SettingsButton => settingsButton;
    }
}
