using System.Collections.Generic;
using ColorBlockJam.UI.Windows;

namespace ColorBlockJam.Settings
{
    public sealed class SettingsPopup : WindowView
    {
        private SettingToggleView[] toggles;

        public IReadOnlyList<SettingToggleView> Toggles => toggles ??= GetComponentsInChildren<SettingToggleView>(true);
    }
}
