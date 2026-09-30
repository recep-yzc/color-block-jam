using System;
using Framework.UI.Views;
using UnityEngine.EventSystems;

namespace Framework.UI.Popups
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
