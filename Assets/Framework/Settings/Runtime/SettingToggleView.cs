using System;
using Framework.UI.Buttons;
using UnityEngine;

namespace Framework.Settings
{
    public sealed class SettingToggleView : MonoBehaviour
    {
        [Tooltip("Bu satırın açıp kapattığı ayar.")]
        [SerializeField] private SettingKind setting;
        [Tooltip("Ayarı açıp kapatan buton.")]
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
