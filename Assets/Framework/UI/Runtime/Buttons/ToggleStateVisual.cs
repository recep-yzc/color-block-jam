using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Strategy for how a <see cref="ToggleButton"/> shows its state. Put one or more under the toggle.
    /// </summary>
    public abstract class ToggleStateVisual : MonoBehaviour
    {
        public abstract void Apply(bool isOn, bool instant);
    }
}
