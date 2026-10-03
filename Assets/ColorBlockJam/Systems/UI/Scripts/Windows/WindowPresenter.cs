using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.UI.Windows
{
    public abstract class WindowPresenter<TView, TResult> : IWindowPresenter where TView : WindowView
    {
        private IWindowHost host;
        private UniTaskCompletionSource<TResult> pending;
        private TResult closedResult;

        protected TView View { get; private set; }

        public bool IsOpen => host != null && host.IsOpen(this);

        protected async UniTask<TResult> OpenAsync(TResult resultWhenClosed, CancellationToken cancellationToken)
        {
            if (host == null)
            {
                throw new InvalidOperationException($"{GetType().Name} has no window host. Get it from {nameof(IWindows)}.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            Settle(isCanceled: false);

            var source = new UniTaskCompletionSource<TResult>();
            pending = source;
            closedResult = resultWhenClosed;

            using (cancellationToken.Register(() =>
                   {
                       if (pending == source)
                       {
                           pending = null;
                       }

                       host.Close(this);
                       source.TrySetCanceled();
                   }))
            {
                await host.ShowAsync(this, cancellationToken);
                return await source.Task;
            }
        }

        protected void Finish(TResult result)
        {
            var source = pending;
            pending = null;
            host?.Close(this);
            source?.TrySetResult(result);
        }

        protected virtual void OnViewCreated()
        {
        }

        protected virtual void OnViewDestroyed()
        {
        }

        protected virtual void OnShowing()
        {
        }

        void IWindowPresenter.Bind(IWindowHost windowHost)
        {
            host = windowHost;
        }

        void IWindowPresenter.Attach(WindowView view)
        {
            View = (TView)view;
            OnViewCreated();
        }

        void IWindowPresenter.Detach()
        {
            OnViewDestroyed();
            View = null;
        }

        void IWindowPresenter.NotifyShowing()
        {
            OnShowing();
        }

        void IWindowPresenter.NotifyClosed(bool isCanceled)
        {
            Settle(isCanceled);
        }

        private void Settle(bool isCanceled)
        {
            var source = pending;
            pending = null;
            if (isCanceled)
            {
                source?.TrySetCanceled();
            }
            else
            {
                source?.TrySetResult(closedResult);
            }
        }
    }
}
