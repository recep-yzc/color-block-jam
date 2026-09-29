using Framework.UI.Buttons;
using Framework.UI.Views;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// The top bar of the home page: lives, coins and the settings button.
    /// </summary>
    public sealed class HomeHudView : UIView
    {
        [SerializeField] private ActionButton settingsButton;
        [SerializeField] private TMP_Text coinsLabel;

        public ActionButton SettingsButton => settingsButton;

        public void SetCoins(int coins)
        {
            coinsLabel.SetText("{0}", coins);
        }
    }
}
