using System.Collections.Generic;
using ColorBlockJam.UI.Popups;

namespace ColorBlockJam.Settings
{
    public sealed class SettingsPopup : Popup
    {
        private SettingToggleView[] toggles;

        public IReadOnlyList<SettingToggleView> Toggles => toggles ??= GetComponentsInChildren<SettingToggleView>(true);
    }
}
