using System;
using ColorBlockJam.UI.Views;
using UnityEngine.EventSystems;

namespace ColorBlockJam.UI.Windows
{
    public sealed class WindowBackdrop : UIView, IPointerClickHandler
    {
        public event Action Clicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke();
        }
    }
}
