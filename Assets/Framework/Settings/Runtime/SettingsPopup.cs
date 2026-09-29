using System.Collections.Generic;
using Framework.UI.Popups;

namespace Framework.Settings
{
    /// <summary>
    /// The settings popup. Its toggle rows are found automatically, so adding a row needs no code.
    /// </summary>
    public sealed class SettingsPopup : Popup
    {
        private SettingToggleView[] toggles;

        public IReadOnlyList<SettingToggleView> Toggles => toggles ??= GetComponentsInChildren<SettingToggleView>(true);
    }
}
