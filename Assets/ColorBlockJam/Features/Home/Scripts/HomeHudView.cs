using Framework.UI.Buttons;
using Framework.UI.Views;
using UnityEngine;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// The top bar of the home page. The coin counter in it is a CoinHud with its own presenter.
    /// </summary>
    public sealed class HomeHudView : UIView
    {
        [SerializeField] private ActionButton settingsButton;

        public ActionButton SettingsButton => settingsButton;
    }
}
