using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    public abstract class ToggleStateVisual : MonoBehaviour
    {
        public abstract void Apply(bool isOn, bool instant);
    }
}
