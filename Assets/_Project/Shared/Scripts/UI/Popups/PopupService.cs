using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Shared.UI.Popups
{
    public sealed class PopupService : IPopupService, IInitializable, ITickable, IDisposable
    {
        private readonly PopupLayer layer;
        private readonly Dictionary<Type, Popup> popupsByType = new();
        private readonly List<Popup> openPopups = new();

        public PopupService(PopupLayer layer)
        {
            this.layer = layer;
        }

        public bool HasOpenPopup => openPopups.Count > 0;

        private Popup TopPopup => openPopups.Count > 0 ? openPopups[openPopups.Count - 1] : null;

        public void Initialize()
        {
            foreach (var popup in layer.Popups)
            {
                popupsByType.Add(popup.GetType(), popup);
                popup.CloseRequested += OnCloseRequested;
                popup.HideImmediate();
            }

            layer.Backdrop.Clicked += OnBackdropClicked;
            layer.Backdrop.HideImmediate();
        }

        public void Dispose()
        {
            foreach (var popup in layer.Popups)
            {
                popup.CloseRequested -= OnCloseRequested;
            }

            layer.Backdrop.Clicked -= OnBackdropClicked;
        }

        public void Tick()
        {
            // Android back button.
            if (Input.GetKeyDown(KeyCode.Escape) && TopPopup != null && TopPopup.CloseOnBackButton)
            {
                TopPopup.RequestClose();
            }
        }

        public UniTask ShowAsync<TPopup>(CancellationToken cancellationToken = default) where TPopup : Popup
        {
            return ShowAsync(Get<TPopup>(), cancellationToken);
        }

        public UniTask HideAsync<TPopup>(CancellationToken cancellationToken = default) where TPopup : Popup
        {
            return HideAsync(Get<TPopup>(), cancellationToken);
        }

        private async UniTask ShowAsync(Popup popup, CancellationToken cancellationToken)
        {
            if (openPopups.Contains(popup))
            {
                return;
            }

            openPopups.Add(popup);
            layer.BringToFront(popup);
            layer.Backdrop.ShowAsync(cancellationToken).Forget();

            await popup.ShowAsync(cancellationToken);
        }

        private async UniTask HideAsync(Popup popup, CancellationToken cancellationToken)
        {
            if (!openPopups.Remove(popup))
            {
                return;
            }

            if (TopPopup != null)
            {
                layer.PlaceBackdropBelow(TopPopup);
            }
            else
            {
                layer.Backdrop.HideAsync(cancellationToken).Forget();
            }

            await popup.HideAsync(cancellationToken);
        }

        private Popup Get<TPopup>() where TPopup : Popup
        {
            if (popupsByType.TryGetValue(typeof(TPopup), out var popup))
            {
                return popup;
            }

            throw new InvalidOperationException($"There is no {typeof(TPopup).Name} under {layer.name}. Add its prefab to the popup layer.");
        }

        private void OnCloseRequested(Popup popup)
        {
            HideAsync(popup, CancellationToken.None).Forget();
        }

        private void OnBackdropClicked()
        {
            if (TopPopup != null && TopPopup.CloseOnBackdropClick)
            {
                TopPopup.RequestClose();
            }
        }
    }
}
