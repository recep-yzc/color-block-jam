using System;
using ColorBlockJam.UI.Views;
using UnityEngine.EventSystems;

namespace ColorBlockJam.UI.Popups
{
    public sealed class PopupBackdrop : UIView, IPointerClickHandler
    {
        public event Action Clicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke();
        }
    }
}
