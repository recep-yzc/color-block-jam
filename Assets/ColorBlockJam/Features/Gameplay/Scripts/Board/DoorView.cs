using System;
using System.Threading;
using ColorBlockJam.Level;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// One door on the board, covering a run of cells along a side. When a block goes through, the door squashes down
    /// to let it pass and springs back up. Its pivot is the middle of its base, so it squashes toward the ground.
    /// </summary>
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

        /// <summary>True when the door covers the cell <paramref name="alongEdge"/> of <paramref name="doorSide"/>.</summary>
        public bool Covers(BoardSide doorSide, int alongEdge)
        {
            return doorSide == side && alongEdge >= start && alongEdge < end;
        }

        /// <summary>Opens for a block going through; a new block restarts it from where it is.</summary>
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
