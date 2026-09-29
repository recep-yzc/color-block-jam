using System;
using VContainer.Unity;

namespace ColorBlockJam.Shared.UI.Views
{
    /// <summary>
    /// Base of the plain C# class that holds the logic of one <see cref="UIView"/>.
    /// The view stays passive: it raises input events and shows data it is given.
    /// Register a presenter as an entry point in the scene's lifetime scope.
    /// </summary>
    public abstract class ViewPresenter<TView> : IInitializable, IDisposable
        where TView : UIView
    {
        protected ViewPresenter(TView view)
        {
            View = view;
        }

        protected TView View { get; }

        void IInitializable.Initialize()
        {
            View.Showing += OnShowing;
            View.Hidden += OnHidden;
            OnInitialize();
        }

        void IDisposable.Dispose()
        {
            View.Showing -= OnShowing;
            View.Hidden -= OnHidden;
            OnDispose();
        }

        /// <summary>Subscribe to the view's input events here.</summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>Unsubscribe from everything <see cref="OnInitialize"/> subscribed to.</summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>Refresh the view with current data here.</summary>
        protected virtual void OnShowing()
        {
        }

        protected virtual void OnHidden()
        {
        }
    }
}
