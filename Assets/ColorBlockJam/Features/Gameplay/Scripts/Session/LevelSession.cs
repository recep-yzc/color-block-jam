using System;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using Cysharp.Threading.Tasks;
using Framework.UI.Popups;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public enum LevelState
    {
        Loading,
        Playing,
        AutoPlaying,
        Won,
        Failed
    }

    /// <summary>
    /// One attempt at one level: runs the timer and decides when the level is won or lost. The level is won when the
    /// board is cleared, and lost when time runs out or when the player is stuck, meaning no sequence of moves can clear
    /// the board. Building the board, watching whether it can be cleared and paying out the result belong to the
    /// classes this one drives. A restart reloads the scene, so a session never has to reset itself.
    /// </summary>
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

            // The data is ready at once, so the HUD can show it before the board is built.
            Level = levels.Load();
            Timer = new LevelTimer(Level.timeLimit);
        }

        public event Action StateChanged;

        public LevelData Level { get; }
        public LevelTimer Timer { get; }
        public int LevelNumber => levels.LevelNumber;

        /// <summary>True when the level editor started this level.</summary>
        public bool IsEditorTest => levels.IsEditorTest;
        public LevelState State { get; private set; }

        private Board Board => levelBoard.Board;

        public void Start()
        {
            levelBoard.Build(Level);
            drag.Attach(Board, levelBoard.Views);
            drag.BlockMoved += OnBlockMoved;
            drag.BlockLeft += OnBlockLeft;
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

            // A popup on screen, such as pause, stops the level.
            var isHeld = popups.HasOpenPopup;
            drag.IsEnabled = State == LevelState.Playing && !isHeld;
            Timer.IsPaused = State != LevelState.Playing || isHeld;

            if (Timer.Tick(Time.deltaTime))
            {
                Fail(LevelFailReason.TimeUp);
            }
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
            drag.BlockMoved -= OnBlockMoved;
            drag.BlockLeft -= OnBlockLeft;
            solvability.FoundUnsolvable -= FailIfStuck;
        }

        /// <summary>Lets the solver finish the level from where the player left it.</summary>
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
            var (isCanceled, isSolved) = await autoPlayer.PlayAsync(Board, levelBoard.Views, OnBlockLeft, lifetime.Token)
                .SuppressCancellationThrow();
            if (isCanceled || isSolved)
            {
                return;
            }

            // The solver gave up within its budget; hand the board back to the player.
            Debug.LogWarning("Auto play found no solution from this board.");
            SetState(LevelState.Playing);
            FailIfStuck();
        }

        private void OnBlockMoved(BoardBlock block)
        {
            hasPlayerMoved = true;
            FailIfStuck();
        }

        private void OnBlockLeft(BoardBlock block, BoardDoor door)
        {
            levelBoard.PlayLeave(door);
            hasPlayerMoved = true;
            if (Board.IsCleared)
            {
                Win();
                return;
            }

            // Fewer blocks make a smaller search, so an unknown answer may now be found.
            solvability.Check(Board);
            FailIfStuck();
        }

        /// <summary>The player sees the fail popup after trying a move, not the moment the level opens.</summary>
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

        private void Fail(LevelFailReason reason)
        {
            if (State is not (LevelState.Playing or LevelState.AutoPlaying))
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
