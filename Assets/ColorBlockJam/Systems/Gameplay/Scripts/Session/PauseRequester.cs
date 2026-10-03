using System;
using System.Threading;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class PauseRequester : IStartable, IDisposable
    {
        private readonly LevelSession session;
        private readonly IWindows windows;
        private readonly ILevelFlow flow;
        private readonly CancellationTokenSource lifetime = new();

        public PauseRequester(LevelSession session, IWindows windows, ILevelFlow flow)
        {
            this.session = session;
            this.windows = windows;
            this.flow = flow;
        }

        public void Start()
        {
            windows.BackPressedWithoutWindow += Request;
            if (!Application.isEditor)
            {
                Application.focusChanged += OnFocusChanged;
            }
        }

        public void Dispose()
        {
            windows.BackPressedWithoutWindow -= Request;
            Application.focusChanged -= OnFocusChanged;
            lifetime.Cancel();
            lifetime.Dispose();
        }

        public void Request()
        {
            if (session.State == LevelState.Playing && !windows.HasOpenWindow)
            {
                PauseAsync().Forget();
            }
        }

        private async UniTaskVoid PauseAsync()
        {
            var (isCanceled, choice) = await windows.Get<PauseMenuPresenter>().ShowAsync(lifetime.Token).SuppressCancellationThrow();
            if (!isCanceled && choice == PauseChoice.Home)
            {
                flow.GoHome();
            }
        }

        private void OnFocusChanged(bool hasFocus)
        {
            if (!hasFocus)
            {
                Request();
            }
        }
    }
}
