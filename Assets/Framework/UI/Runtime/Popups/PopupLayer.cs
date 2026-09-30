using UnityEngine;

namespace Framework.UI.Popups
{
    /// <summary>
    /// Scene parent of the popup instances and their backdrop.
    /// It has its own canvas, so popup animations do not rebuild the rest of the screen.
    /// </summary>
    public sealed class PopupLayer : MonoBehaviour
    {
        [Tooltip("Popup açıkken arkasındaki karartma.")]
        [SerializeField] private PopupBackdrop backdrop;

        public PopupBackdrop Backdrop => backdrop;

        /// <summary>Puts the popup on top of the others and the backdrop right below it.</summary>
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
