using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.UI.Windows
{
    public abstract class WindowPresenter<TView> : IWindowPresenter where TView : WindowView
    {
        private IWindowHost host;
        private object pending;
        private Action<bool> ending;

        protected TView View { get; private set; }

        public bool IsOpen => host != null && host.IsOpen(this);

        protected async UniTask<TResult> OpenAsync<TResult>(TResult closedResult, CancellationToken cancellationToken)
        {
            if (host == null)
            {
                throw new InvalidOperationException($"{GetType().Name} has no window host. Get it from {nameof(IWindows)}.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            ending?.Invoke(false);

            var source = new UniTaskCompletionSource<TResult>();
            pending = source;
            ending = isCanceled =>
            {
                if (isCanceled)
                {
                    source.TrySetCanceled();
                }
                else
                {
                    source.TrySetResult(closedResult);
                }
            };

            using (cancellationToken.Register(() =>
                   {
                       Forget(source);
                       host.Close(this);
                       source.TrySetCanceled();
                   }))
            {
                await host.ShowAsync(this, cancellationToken);
                return await source.Task;
            }
        }

        protected void Finish<TResult>(TResult result)
        {
            var source = pending as UniTaskCompletionSource<TResult>;
            Forget(source);
            host?.Close(this);
            source?.TrySetResult(result);
        }

        private void Forget(object source)
        {
            if (source != null && ReferenceEquals(pending, source))
            {
                pending = null;
                ending = null;
            }
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
            var end = ending;
            ending = null;
            pending = null;
            end?.Invoke(isCanceled);
        }
    }
}
