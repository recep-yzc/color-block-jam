using System;
using Framework.UI.Buttons;
using UnityEngine;

namespace Framework.Settings
{
    /// <summary>
    /// One row of the settings popup: a toggle bound to one <see cref="SettingKind"/>.
    /// </summary>
    public sealed class SettingToggleView : MonoBehaviour
    {
        [SerializeField] private SettingKind setting;
        [SerializeField] private ToggleButton toggle;

        public event Action<SettingKind, bool> Changed;

        public SettingKind Setting => setting;

        private void Awake()
        {
            toggle.ValueChanged += OnToggleValueChanged;
        }

        private void OnDestroy()
        {
            toggle.ValueChanged -= OnToggleValueChanged;
        }

        public void Show(bool isOn)
        {
            toggle.SetIsOn(isOn, notify: false, instant: true);
        }

        private void OnToggleValueChanged(bool isOn)
        {
            Changed?.Invoke(setting, isOn);
        }
    }
}
