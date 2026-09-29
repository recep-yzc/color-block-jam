using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.UI.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Framework.UI.Popups
{
    public sealed class PopupService : IPopupService, IInitializable, ITickable, IDisposable
    {
        private readonly PopupLayer layer;
        private readonly PopupCatalog catalog;
        private readonly LifetimeScope ownerScope;
        private readonly Dictionary<Type, Popup> instances = new();
        private readonly List<Popup> openPopups = new();

        public PopupService(PopupLayer layer, PopupCatalog catalog, IObjectResolver resolver)
        {
            this.layer = layer;
            this.catalog = catalog;

            // The scope that built this service. Popups with their own scope become its children,
            // so their presenters can use the scene's services.
            ownerScope = resolver.ApplicationOrigin as LifetimeScope;
        }

        public bool HasOpenPopup => openPopups.Count > 0;

        private Popup TopPopup => openPopups.Count > 0 ? openPopups[openPopups.Count - 1] : null;

        public void Initialize()
        {
            layer.Backdrop.Clicked += OnBackdropClicked;
            layer.Backdrop.HideImmediate();
        }

        public void Dispose()
        {
            layer.Backdrop.Clicked -= OnBackdropClicked;

            foreach (var popup in instances.Values)
            {
                popup.CloseRequested -= OnCloseRequested;
            }
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
            return ShowAsync(GetOrCreate(typeof(TPopup)), cancellationToken);
        }

        public UniTask HideAsync<TPopup>(CancellationToken cancellationToken = default) where TPopup : Popup
        {
            return instances.TryGetValue(typeof(TPopup), out var popup)
                ? HideAsync(popup, cancellationToken)
                : UniTask.CompletedTask;
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

            // Skip when the popup was opened again during its hide transition.
            if (popup.DestroyOnHide && popup.State == ViewState.Hidden)
            {
                Destroy(popup);
            }
        }

        private Popup GetOrCreate(Type popupType)
        {
            if (instances.TryGetValue(popupType, out var popup))
            {
                return popup;
            }

            using (LifetimeScope.EnqueueParent(ownerScope))
            {
                popup = Object.Instantiate(catalog.GetPrefab(popupType), layer.transform);
            }

            popup.HideImmediate();
            popup.CloseRequested += OnCloseRequested;
            instances.Add(popupType, popup);
            return popup;
        }

        private void Destroy(Popup popup)
        {
            instances.Remove(popup.GetType());
            popup.CloseRequested -= OnCloseRequested;
            Object.Destroy(popup.gameObject);
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
