using UnityEngine;

namespace Framework.UI.Buttons
{
    public abstract class ToggleStateVisual : MonoBehaviour
    {
        public abstract void Apply(bool isOn, bool instant);
    }
}
