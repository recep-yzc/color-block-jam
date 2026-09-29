using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.Shared.UI.Popups
{
    /// <summary>
    /// Scene root of the popups. Drop popup prefabs under it; they are registered by type automatically.
    /// It has its own canvas, so popup animations do not rebuild the rest of the screen.
    /// </summary>
    public sealed class PopupLayer : MonoBehaviour
    {
        [SerializeField] private PopupBackdrop backdrop;

        private Popup[] popups;

        public PopupBackdrop Backdrop => backdrop;
        public IReadOnlyList<Popup> Popups => popups ??= GetComponentsInChildren<Popup>(true);

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
