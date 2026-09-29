using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Shows one set of objects while the toggle is on and another while it is off.
    /// </summary>
    public sealed class ToggleObjectsVisual : ToggleStateVisual
    {
        [SerializeField] private GameObject[] onObjects;
        [SerializeField] private GameObject[] offObjects;

        public override void Apply(bool isOn, bool instant)
        {
            foreach (var onObject in onObjects)
            {
                onObject.SetActive(isOn);
            }

            foreach (var offObject in offObjects)
            {
                offObject.SetActive(!isOn);
            }
        }
    }
}
