using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Framework.UI.Popups
{
    /// <summary>
    /// Opens and closes the popups of the current scene as a stack: the last opened popup is on top.
    /// </summary>
    public interface IPopupService
    {
        /// <summary>
        /// Raised when the back button is pressed and no popup is open to close, so the screen can decide what back
        /// means there, for example pausing a level.
        /// </summary>
        event Action BackPressedWithoutPopup;

        bool HasOpenPopup { get; }

        UniTask ShowAsync<TPopup>(CancellationToken cancellationToken = default) where TPopup : Popup;

        UniTask HideAsync<TPopup>(CancellationToken cancellationToken = default) where TPopup : Popup;
    }
}
