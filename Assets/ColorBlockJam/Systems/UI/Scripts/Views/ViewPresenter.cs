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
            OnInitialize();
        }

        void IDisposable.Dispose()
        {
            OnDispose();
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnDispose()
        {
        }
    }
}
