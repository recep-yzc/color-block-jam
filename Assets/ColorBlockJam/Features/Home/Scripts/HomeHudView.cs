using Framework.UI.Buttons;
using Framework.UI.Views;
using UnityEngine;

namespace ColorBlockJam.Home
{
    public sealed class HomeHudView : UIView
    {
        [Tooltip("Ayarlar popup'ını açan buton.")]
        [SerializeField] private ActionButton settingsButton;

        public ActionButton SettingsButton => settingsButton;
    }
}
