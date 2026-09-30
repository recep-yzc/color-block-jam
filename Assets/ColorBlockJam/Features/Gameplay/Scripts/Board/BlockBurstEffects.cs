using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.Pooling;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class BlockBurstEffects : IDisposable
    {
        private readonly ComponentPool<ParticleSystem> pool;
        private readonly Transform root;
        private readonly CancellationTokenSource lifetime = new();

        public BlockBurstEffects(ParticleSystem prefab, Transform root, int prewarm)
        {
            this.root = root;
            pool = new ComponentPool<ParticleSystem>(prefab, root);
            pool.Prewarm(prewarm);
        }

        public void Play(Vector3 position, Color color)
        {
            var burst = pool.Get(root);
            burst.transform.position = position;
            var main = burst.main;
            main.startColor = color;
            burst.Play(withChildren: true);
            ReleaseWhenDoneAsync(burst, main.duration + main.startLifetime.constantMax).Forget();
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
            pool.Dispose();
        }

        private async UniTaskVoid ReleaseWhenDoneAsync(ParticleSystem burst, float seconds)
        {
            var canceled = await UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: true, cancellationToken: lifetime.Token)
                .SuppressCancellationThrow();
            if (!canceled)
            {
                pool.Release(burst);
            }
        }
    }
}
