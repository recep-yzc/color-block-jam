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
        private readonly CancellationTokenSource lifetime = new();

        public LevelIntros(IReadOnlyList<ILevelIntro> intros, ILevelProvider levels)
        {
            this.intros = intros;
            this.levels = levels;
        }

        public void Start()
        {
            if (intros.Count > 0 && !levels.IsEditorTest)
            {
                PresentAsync().Forget();
            }
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }

        private async UniTaskVoid PresentAsync()
        {
            var token = lifetime.Token;
            foreach (var intro in intros)
            {
                if (await intro.PresentAsync(token).SuppressCancellationThrow())
                {
                    return;
                }
            }
        }
    }
}
