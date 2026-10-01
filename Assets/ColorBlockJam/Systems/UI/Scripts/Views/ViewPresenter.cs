using System;
using VContainer.Unity;

namespace ColorBlockJam.UI.Views
{
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

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnDispose()
        {
        }

        protected virtual void OnShowing()
        {
        }

        protected virtual void OnHidden()
        {
        }
    }
}
