using System.Collections.Generic;
using ColorBlockJam.UI.Buttons;
using ColorBlockJam.UI.Windows;
using UnityEngine;

namespace ColorBlockJam.Settings
{
    public sealed class SettingsPopup : WindowView
    {
        [Tooltip("Oyundan ana ekrana dönen buton. Ana ekrandaki ayarlarda boş kalır.")]
        [SerializeField] private ActionButton homeButton;

        private SettingToggleView[] toggles;

        public IReadOnlyList<SettingToggleView> Toggles => toggles ??= GetComponentsInChildren<SettingToggleView>(true);
        public ActionButton HomeButton => homeButton;
    }
}
