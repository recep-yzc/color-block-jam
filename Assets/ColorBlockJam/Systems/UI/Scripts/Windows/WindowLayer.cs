using UnityEngine;

namespace ColorBlockJam.UI.Windows
{
    public sealed class WindowLayer : MonoBehaviour
    {
        [Tooltip("Pencere açıkken arkasındaki karartma.")]
        [SerializeField] private WindowBackdrop backdrop;
        [Tooltip("Katmanın tamamını birlikte saydamlaştıran grup. Pencerenin arkasındaki oyuna bakarken kullanılır.")]
        [SerializeField] private CanvasGroup group;

        public WindowBackdrop Backdrop => backdrop;
        public CanvasGroup Group => group;

        public void BringToFront(WindowView window)
        {
            window.transform.SetAsLastSibling();
        }

        public void PlaceBackdropBelow(WindowView window)
        {
            var windowIndex = window.transform.GetSiblingIndex();
            var backdropIndex = backdrop.transform.GetSiblingIndex();
            backdrop.transform.SetSiblingIndex(backdropIndex < windowIndex ? windowIndex - 1 : windowIndex);
        }
    }
}
