using System;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// An on/off button. How each state looks is decided by the <see cref="ToggleStateVisual"/>
    /// components under it, so the same toggle works with any design.
    /// </summary>
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

        /// <param name="notify">Raise <see cref="ValueChanged"/>. Use false when showing saved data.</param>
        /// <param name="instant">Skip the visual transition.</param>
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
