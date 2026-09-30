using UnityEngine;

namespace Framework.UI.Popups
{
    public sealed class PopupLayer : MonoBehaviour
    {
        [Tooltip("Popup açıkken arkasındaki karartma.")]
        [SerializeField] private PopupBackdrop backdrop;

        public PopupBackdrop Backdrop => backdrop;

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
