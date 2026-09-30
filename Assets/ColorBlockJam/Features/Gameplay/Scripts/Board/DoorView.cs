using System;
using System.Threading;
using ColorBlockJam.Level;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class DoorView : MonoBehaviour
    {
        private GameplayConfig config;
        private BoardSide side;
        private int start;
        private int end;
        private CancellationTokenSource playback;

        public void Initialize(BoardSide doorSide, int from, int to, GameplayConfig gameplayConfig)
        {
            side = doorSide;
            start = from;
            end = to;
            config = gameplayConfig;
        }

        public bool Covers(BoardSide doorSide, int alongEdge)
        {
            return doorSide == side && alongEdge >= start && alongEdge < end;
        }

        public void PlayEntry()
        {
            playback?.Cancel();
            playback?.Dispose();
            playback = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            PlayEntryAsync(playback.Token).Forget();
        }

        private void OnDestroy()
        {
            playback?.Cancel();
            playback?.Dispose();
        }

        private async UniTaskVoid PlayEntryAsync(CancellationToken cancellationToken)
        {
            var open = new Vector3(config.DoorOpenWiden, config.DoorOpenSquash, config.DoorOpenWiden);

            var isCanceled = await LMotion.Create(transform.localScale, open, config.DoorOpenDuration)
                .WithEase(Ease.OutQuad)
                .BindToLocalScale(transform)
                .AddTo(this)
                .ToUniTask(cancellationToken)
                .SuppressCancellationThrow();
            if (isCanceled)
            {
                return;
            }

            isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(config.DoorHoldDuration), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
            if (isCanceled)
            {
                return;
            }

            await LMotion.Create(open, Vector3.one, config.DoorCloseDuration)
                .WithEase(Ease.OutBack)
                .BindToLocalScale(transform)
                .AddTo(this)
                .ToUniTask(cancellationToken)
                .SuppressCancellationThrow();
        }
    }
}
