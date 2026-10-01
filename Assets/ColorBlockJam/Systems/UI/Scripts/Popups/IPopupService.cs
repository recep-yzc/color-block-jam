using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.UI.Popups
{
    public interface IPopupService
    {
        event Action BackPressedWithoutPopup;

        bool HasOpenPopup { get; }

        UniTask ShowAsync<TPopup>(CancellationToken cancellationToken = default) where TPopup : Popup;

        UniTask HideAsync<TPopup>(CancellationToken cancellationToken = default) where TPopup : Popup;
    }
}
