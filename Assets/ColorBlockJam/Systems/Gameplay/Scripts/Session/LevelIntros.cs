using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelIntros : IStartable, IDisposable
    {
        private readonly IReadOnlyList<ILevelIntro> intros;
        private readonly ILevelProvider levels;
        private readonly GameplayConfig config;
        private readonly CancellationTokenSource lifetime = new();

        public LevelIntros(IReadOnlyList<ILevelIntro> intros, ILevelProvider levels, GameplayConfig config)
        {
            this.intros = intros;
            this.levels = levels;
            this.config = config;
        }

        public bool IsShowing { get; private set; }

        public void Start()
        {
            if (levels.IsEditorTest || !HasPending())
            {
                return;
            }

            IsShowing = true;
            PresentAsync().Forget();
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }

        private bool HasPending()
        {
            foreach (var intro in intros)
            {
                if (intro.IsPending)
                {
                    return true;
                }
            }

            return false;
        }

        private async UniTaskVoid PresentAsync()
        {
            var token = lifetime.Token;
            if (await UniTask.Delay(TimeSpan.FromSeconds(config.IntroDelay), cancellationToken: token).SuppressCancellationThrow())
            {
                return;
            }

            foreach (var intro in intros)
            {
                if (intro.IsPending && await intro.PresentAsync(token).SuppressCancellationThrow())
                {
                    return;
                }
            }

            IsShowing = false;
        }
    }
}
