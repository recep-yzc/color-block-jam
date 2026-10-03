using System;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelSession : IStartable, ITickable, IDisposable
    {
        private readonly ILevelProvider levels;
        private readonly LevelBoard levelBoard;
        private readonly BlockDragController drag;
        private readonly AutoPlayer autoPlayer;
        private readonly SolvabilityWatcher solvability;
        private readonly LevelResults results;
        private readonly IWindows windows;
        private readonly ILevelFlow flow;
        private readonly LevelIntros intros;
        private readonly CancellationTokenSource lifetime = new();
        private bool hasPlayerMoved;

        public LevelSession(LevelData level, ILevelProvider levels, LevelBoard levelBoard, BlockDragController drag, AutoPlayer autoPlayer,
            SolvabilityWatcher solvability, LevelResults results, IWindows windows, ILevelFlow flow, LevelIntros intros)
        {
            this.levels = levels;
            this.levelBoard = levelBoard;
            this.drag = drag;
            this.autoPlayer = autoPlayer;
            this.solvability = solvability;
            this.results = results;
            this.windows = windows;
            this.flow = flow;
            this.intros = intros;

            Level = level;
            Timer = new LevelTimer(Level.timeLimit);
        }

        public event Action StateChanged;

        public LevelData Level { get; }
        public LevelTimer Timer { get; }
        public int LevelNumber => levels.LevelNumber;

        public bool IsEditorTest => levels.IsEditorTest;
        public LevelState State { get; private set; }

        private Board Board => levelBoard.Board;

        public void Start()
        {
            levelBoard.Build(Level);
            drag.BlockMoved += OnBlockMoved;
            levelBoard.BlockLeft += OnBlockLeft;
            levelBoard.BlocksSmashed += OnBlocksSmashed;
            solvability.FoundUnsolvable += FailIfStuck;

            SetState(LevelState.Playing);
            solvability.Check(Board);
        }

        public void Tick()
        {
            if (State == LevelState.Loading)
            {
                return;
            }

            var isHeld = windows.HasOpenWindow || flow.IsLeaving || intros.IsShowing;
            drag.IsEnabled = State == LevelState.Playing && !isHeld;
            Timer.IsPaused = State != LevelState.Playing || isHeld;

            if (Timer.Tick(Time.deltaTime))
            {
                RunOutOfTime();
            }
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
            drag.BlockMoved -= OnBlockMoved;
            levelBoard.BlockLeft -= OnBlockLeft;
            levelBoard.BlocksSmashed -= OnBlocksSmashed;
            solvability.FoundUnsolvable -= FailIfStuck;
        }

        public void StartAutoPlay()
        {
            if (State != LevelState.Playing)
            {
                return;
            }

            SetState(LevelState.AutoPlaying);
            AutoPlayAsync().Forget();
        }

        private async UniTaskVoid AutoPlayAsync()
        {
            var (isCanceled, isSolved) = await autoPlayer.PlayAsync(lifetime.Token).SuppressCancellationThrow();
            if (isCanceled || isSolved)
            {
                return;
            }

            Debug.LogWarning("Auto play found no solution from this board.");
            SetState(LevelState.Playing);
            FailIfStuck();
        }

        private void OnBlockMoved()
        {
            hasPlayerMoved = true;
            FailIfStuck();
        }

        private void OnBlockLeft()
        {
            hasPlayerMoved = true;
            if (Board.IsCleared)
            {
                Win();
                return;
            }

            solvability.Check(Board);
            FailIfStuck();
        }

        private void OnBlocksSmashed()
        {
            hasPlayerMoved = true;
            if (Board.IsCleared)
            {
                Win();
                return;
            }

            solvability.Recheck(Board);
            FailIfStuck();
        }

        private void FailIfStuck()
        {
            if (solvability.IsUnsolvable && hasPlayerMoved && State == LevelState.Playing)
            {
                Fail(LevelFailReason.Stuck);
            }
        }

        private void Win()
        {
            if (State is not (LevelState.Playing or LevelState.AutoPlaying))
            {
                return;
            }

            SetState(LevelState.Won);
            results.Win(Level.difficulty);
        }

        private void RunOutOfTime()
        {
            if (State != LevelState.Playing)
            {
                return;
            }

            SetState(LevelState.OutOfTime);
            OfferExtraTimeAsync().Forget();
        }

        private async UniTaskVoid OfferExtraTimeAsync()
        {
            var (isCanceled, seconds) = await results.OfferExtraTimeAsync(lifetime.Token).SuppressCancellationThrow();
            if (isCanceled)
            {
                return;
            }

            if (seconds > 0)
            {
                AddExtraTime(seconds);
            }
            else
            {
                DeclineExtraTime();
            }
        }

        private void AddExtraTime(float seconds)
        {
            if (State != LevelState.OutOfTime)
            {
                return;
            }

            Timer.Add(seconds);
            SetState(LevelState.Playing);
        }

        private void DeclineExtraTime()
        {
            if (State == LevelState.OutOfTime)
            {
                Fail(LevelFailReason.TimeUp);
            }
        }

        private void Fail(LevelFailReason reason)
        {
            if (State is not (LevelState.Playing or LevelState.AutoPlaying or LevelState.OutOfTime))
            {
                return;
            }

            SetState(LevelState.Failed);
            results.Fail(reason);
        }

        private void SetState(LevelState state)
        {
            State = state;
            drag.IsEnabled = state == LevelState.Playing;
            StateChanged?.Invoke();
        }
    }
}
