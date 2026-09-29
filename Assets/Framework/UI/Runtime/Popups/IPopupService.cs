using System.Threading;
using Cysharp.Threading.Tasks;

namespace Framework.UI.Popups
{
    /// <summary>
    /// Opens and closes the popups of the current scene as a stack: the last opened popup is on top.
    /// </summary>
    public interface IPopupService
    {
        bool HasOpenPopup { get; }

        UniTask ShowAsync<TPopup>(CancellationToken cancellationToken = default) where TPopup : Popup;

        UniTask HideAsync<TPopup>(CancellationToken cancellationToken = default) where TPopup : Popup;
    }
}
