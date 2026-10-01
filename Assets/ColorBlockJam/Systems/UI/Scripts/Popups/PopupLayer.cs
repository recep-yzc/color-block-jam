using UnityEngine;

namespace ColorBlockJam.UI.Popups
{
    public sealed class PopupLayer : MonoBehaviour
    {
        [Tooltip("Popup açıkken arkasındaki karartma.")]
        [SerializeField] private PopupBackdrop backdrop;
        [Tooltip("Katmanın tamamını birlikte saydamlaştıran grup. Popup'ın arkasındaki oyuna bakarken kullanılır.")]
        [SerializeField] private CanvasGroup group;

        public PopupBackdrop Backdrop => backdrop;
        public CanvasGroup Group => group;

        public void BringToFront(Popup popup)
        {
            popup.transform.SetAsLastSibling();
            PlaceBackdropBelow(popup);
        }

        public void PlaceBackdropBelow(Popup popup)
        {
            var popupIndex = popup.transform.GetSiblingIndex();
            var backdropIndex = backdrop.transform.GetSiblingIndex();
            backdrop.transform.SetSiblingIndex(backdropIndex < popupIndex ? popupIndex - 1 : popupIndex);
        }
    }
}
