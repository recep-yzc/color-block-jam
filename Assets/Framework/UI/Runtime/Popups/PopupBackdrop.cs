using System;
using Framework.UI.Views;
using UnityEngine.EventSystems;

namespace Framework.UI.Popups
{
    /// <summary>
    /// The dark layer behind the top popup. It blocks input to the screen below and reports taps.
    /// </summary>
    public sealed class PopupBackdrop : UIView, IPointerClickHandler
    {
        public event Action Clicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke();
        }
    }
}
