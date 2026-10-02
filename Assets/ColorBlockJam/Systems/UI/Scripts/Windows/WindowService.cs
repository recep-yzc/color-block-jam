using System;
using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.UI.Views;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace ColorBlockJam.UI.Windows
{
    public sealed class WindowService : IWindows, IWindowHost, IInitializable, ITickable, IDisposable
    {
        private sealed class Slot
        {
            public WindowEntry Entry;
            public WindowView View;
        }

        private readonly WindowCatalog catalog;
        private readonly WindowLayer layerPrefab;
        private readonly IObjectResolver resolver;
        private readonly Dictionary<IWindowPresenter, Slot> slots = new();
        private readonly List<IWindowPresenter> openWindows = new();
        private readonly InputAction backAction = new("Back", InputActionType.Button, "<Keyboard>/escape");
        private WindowLayer layer;

        public WindowService(WindowCatalog catalog, WindowLayer layerPrefab, IObjectResolver resolver)
        {
            this.catalog = catalog;
            this.layerPrefab = layerPrefab;
            this.resolver = resolver;
        }

        public event Action BackPressedWithoutWindow;

        public bool HasOpenWindow => openWindows.Count > 0;

        private IWindowPresenter Top => openWindows.Count > 0 ? openWindows[openWindows.Count - 1] : null;

        public void Initialize()
        {
            layer = Object.Instantiate(layerPrefab);
            layer.name = layerPrefab.name;
            Object.DontDestroyOnLoad(layer.gameObject);
            layer.Backdrop.Clicked += OnBackdropClicked;
            layer.Backdrop.HideImmediate();
            backAction.Enable();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        public void Dispose()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            backAction.Dispose();
            if (layer != null)
            {
                layer.Backdrop.Clicked -= OnBackdropClicked;
            }
        }

        public void Tick()
        {
            if (!backAction.WasPressedThisFrame())
            {
                return;
            }

            var top = Top;
            if (top == null)
            {
                BackPressedWithoutWindow?.Invoke();
            }
            else if (slots[top].Entry.settings.closeOnBackButton)
            {
                Close(top);
            }
        }

        public TPresenter Get<TPresenter>() where TPresenter : class, IWindowPresenter
        {
            var presenter = resolver.Resolve<TPresenter>();
            if (!slots.ContainsKey(presenter))
            {
                slots.Add(presenter, new Slot { Entry = catalog.EntryFor(typeof(TPresenter)) });
                presenter.Bind(this);
            }

            return presenter;
        }

        public bool IsOpen(IWindowPresenter presenter)
        {
            return openWindows.Contains(presenter);
        }

        public async UniTask ShowAsync(IWindowPresenter presenter, CancellationToken cancellationToken)
        {
            if (openWindows.Contains(presenter))
            {
                return;
            }

            var slot = slots[presenter];
            if (slot.View == null)
            {
                Create(presenter, slot);
            }

            openWindows.Add(presenter);
            layer.BringToFront(slot.View);
            ShowBackdrop();
            presenter.NotifyShowing();
            await slot.View.ShowAsync(cancellationToken);
        }

        public void Close(IWindowPresenter presenter)
        {
            CloseAsync(presenter).Forget();
        }

        private async UniTaskVoid CloseAsync(IWindowPresenter presenter)
        {
            if (!openWindows.Remove(presenter))
            {
                return;
            }

            var slot = slots[presenter];
            ShowBackdrop();
            await slot.View.HideAsync();
            if (slot.View == null || slot.View.State != ViewState.Hidden)
            {
                return;
            }

            if (slot.Entry.settings.destroyOnHide)
            {
                Destroy(presenter, slot);
            }

            presenter.NotifyClosed(isCanceled: false);
        }

        private void CloseNow(IWindowPresenter presenter)
        {
            if (!openWindows.Remove(presenter))
            {
                return;
            }

            var slot = slots[presenter];
            slot.View.HideImmediate();
            if (slot.Entry.settings.destroyOnHide)
            {
                Destroy(presenter, slot);
            }

            presenter.NotifyClosed(isCanceled: true);
        }

        private void Create(IWindowPresenter presenter, Slot slot)
        {
            var view = Object.Instantiate(slot.Entry.prefab, layer.transform);
            view.name = slot.Entry.prefab.name;
            view.UseTransitions(slot.Entry.settings.showTransition, slot.Entry.settings.hideTransition);
            view.HideImmediate();
            view.CloseRequested += OnCloseRequested;
            slot.View = view;
            presenter.Attach(view);
        }

        private void Destroy(IWindowPresenter presenter, Slot slot)
        {
            presenter.Detach();
            slot.View.CloseRequested -= OnCloseRequested;
            Object.Destroy(slot.View.gameObject);
            slot.View = null;
        }

        private void ShowBackdrop()
        {
            for (var i = openWindows.Count - 1; i >= 0; i--)
            {
                var slot = slots[openWindows[i]];
                if (slot.Entry.settings.dimsBackground)
                {
                    layer.PlaceBackdropBelow(slot.View);
                    layer.Backdrop.ShowAsync().Forget();
                    return;
                }
            }

            layer.Backdrop.HideAsync().Forget();
        }

        private void OnCloseRequested(WindowView view)
        {
            foreach (var pair in slots)
            {
                if (pair.Value.View == view)
                {
                    Close(pair.Key);
                    return;
                }
            }
        }

        private void OnBackdropClicked()
        {
            var top = Top;
            if (top != null && slots[top].Entry.settings.closeOnBackdropClick)
            {
                Close(top);
            }
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            for (var i = openWindows.Count - 1; i >= 0; i--)
            {
                var presenter = openWindows[i];
                if (slots[presenter].Entry.settings.closeOnSceneChange)
                {
                    CloseNow(presenter);
                }
            }

            if (openWindows.Count == 0)
            {
                layer.Backdrop.HideImmediate();
            }
        }
    }
}
