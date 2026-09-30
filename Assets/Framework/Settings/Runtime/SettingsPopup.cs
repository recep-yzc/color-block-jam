using System.Collections.Generic;
using Framework.UI.Popups;

namespace Framework.Settings
{
    public sealed class SettingsPopup : Popup
    {
        private SettingToggleView[] toggles;

        public IReadOnlyList<SettingToggleView> Toggles => toggles ??= GetComponentsInChildren<SettingToggleView>(true);
    }
}
