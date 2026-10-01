using System;
using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    public sealed class ToggleButton : ButtonBase
    {
        [Tooltip("Toggle'ın açık mı kapalı mı olduğu.")]
        [SerializeField] private bool isOn = true;

        private ToggleStateVisual[] visuals;

        public event Action<bool> ValueChanged;

        public bool IsOn => isOn;

        private ToggleStateVisual[] Visuals => visuals ??= GetComponentsInChildren<ToggleStateVisual>(true);

        protected override void Awake()
        {
            base.Awake();
            ApplyVisuals(instant: true);
        }

        public void SetIsOn(bool value, bool notify, bool instant)
        {
            if (isOn == value)
            {
                return;
            }

            isOn = value;
            ApplyVisuals(instant);

            if (notify)
            {
                ValueChanged?.Invoke(isOn);
            }
        }

        protected override void OnClick()
        {
            SetIsOn(!isOn, notify: true, instant: false);
        }

        private void ApplyVisuals(bool instant)
        {
            foreach (var visual in Visuals)
            {
                visual.Apply(isOn, instant);
            }
        }
    }
}
