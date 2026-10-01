using System;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using ColorBlockJam.UI.Popups;
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
        private readonly IPopupService popups;
        private readonly CancellationTokenSource lifetime = new();
        private bool hasPlayerMoved;

        public LevelSession(ILevelProvider levels, LevelBoard levelBoard, BlockDragController drag, AutoPlayer autoPlayer,
            SolvabilityWatcher solvability, LevelResults results, IPopupService popups)
        {
            this.levels = levels;
            this.levelBoard = levelBoard;
            this.drag = drag;
            this.autoPlayer = autoPlayer;
            this.solvability = solvability;
            this.results = results;
            this.popups = popups;

            Level = levels.Load();
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
            drag.Attach(Board, levelBoard.Views);
            drag.BlockMoved += OnBlockMoved;
            drag.BlockLeft += OnBlockCleared;
            levelBoard.BlockSmashed += OnBlockSmashed;
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

            var isHeld = popups.HasOpenPopup;
            drag.IsEnabled = State == LevelState.Playing && !isHeld;
            Timer.IsPaused = State != LevelState.Playing || isHeld;

            if (Timer.Tick(Time.deltaTime))
            {
                RunOutOfTime();
            }
        }

        public void AddExtraTime(float seconds)
        {
            if (State != LevelState.OutOfTime)
            {
                return;
            }

            Timer.Add(seconds);
            SetState(LevelState.Playing);
        }

        public void DeclineExtraTime()
        {
            if (State == LevelState.OutOfTime)
            {
                Fail(LevelFailReason.TimeUp);
            }
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
            drag.BlockMoved -= OnBlockMoved;
            drag.BlockLeft -= OnBlockCleared;
            levelBoard.BlockSmashed -= OnBlockSmashed;
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
            var (isCanceled, isSolved) = await autoPlayer.PlayAsync(Board, levelBoard.Views, OnBlockCleared, lifetime.Token)
                .SuppressCancellationThrow();
            if (isCanceled || isSolved)
            {
                return;
            }

            Debug.LogWarning("Auto play found no solution from this board.");
            SetState(LevelState.Playing);
            FailIfStuck();
        }

        private void OnBlockMoved(BoardBlock block)
        {
            hasPlayerMoved = true;
            FailIfStuck();
        }

        private void OnBlockCleared(BoardBlock block, BoardDoor door)
        {
            levelBoard.ShowBlockCleared(door);
            hasPlayerMoved = true;
            if (Board.IsCleared)
            {
                Win();
                return;
            }

            solvability.Check(Board);
            FailIfStuck();
        }

        private void OnBlockSmashed(BoardBlock block)
        {
            OnBlockCleared(block, null);
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
            results.Win();
        }

        private void RunOutOfTime()
        {
            if (State != LevelState.Playing)
            {
                return;
            }

            SetState(LevelState.OutOfTime);
            results.OfferExtraTime();
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
