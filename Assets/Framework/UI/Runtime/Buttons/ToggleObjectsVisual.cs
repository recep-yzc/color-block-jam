using UnityEngine;

namespace Framework.UI.Buttons
{
    public sealed class ToggleObjectsVisual : ToggleStateVisual
    {
        [Tooltip("Sadece açıkken görünen objeler.")]
        [SerializeField] private GameObject[] onObjects;
        [Tooltip("Sadece kapalıyken görünen objeler.")]
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
