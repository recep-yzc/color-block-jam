using System;
using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Economy;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using ColorBlockJam.Progression;
using Cysharp.Threading.Tasks;
using Framework.Settings;
using Framework.UI.Popups;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

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
    /// One attempt at one level: builds the board, runs the timer and decides when the level is won or lost.
    /// The level is lost when time runs out or when no sequence of moves can clear the board any more.
    /// A restart reloads the scene, so a session never has to reset itself.
    /// </summary>
    public sealed class LevelSession : IStartable, ITickable, IDisposable
    {
        private readonly ILevelProvider levels;
        private readonly BoardView boardView;
        private readonly BoardArt art;
        private readonly BlockPalette palette;
        private readonly GameplayConfig config;
        private readonly BoardCamera boardCamera;
        private readonly BlockDragController drag;
        private readonly BoardSolver solver;
        private readonly AutoPlayer autoPlayer;
        private readonly BlockBurstEffects bursts;
        private readonly IPopupService popups;
        private readonly IProgressionService progression;
        private readonly ICoinWallet wallet;
        private readonly EconomyConfig economy;
        private readonly IHapticService haptics;
        private readonly LevelOutcome outcome;
        private readonly List<BlockView> views = new();
        private readonly CancellationTokenSource lifetime = new();

        private ColorMaterials blockMaterials;
        private ColorMaterials doorMaterials;
        private Board board;

        public LevelSession(ILevelProvider levels, BoardView boardView, BoardArt art, BlockPalette palette, GameplayConfig config,
            BoardCamera boardCamera, BlockDragController drag, BoardSolver solver, AutoPlayer autoPlayer, BlockBurstEffects bursts,
            IPopupService popups, IProgressionService progression, ICoinWallet wallet, EconomyConfig economy, IHapticService haptics,
            LevelOutcome outcome)
        {
            this.levels = levels;
            this.boardView = boardView;
            this.art = art;
            this.palette = palette;
            this.config = config;
            this.boardCamera = boardCamera;
            this.drag = drag;
            this.solver = solver;
            this.autoPlayer = autoPlayer;
            this.bursts = bursts;
            this.popups = popups;
            this.progression = progression;
            this.wallet = wallet;
            this.economy = economy;
            this.haptics = haptics;
            this.outcome = outcome;

            // The data is ready at once, so the HUD can show it before the board is built.
            Level = levels.Load();
            Timer = new LevelTimer(Level.timeLimit);
        }

        public event Action StateChanged;

        public LevelData Level { get; }
        public LevelTimer Timer { get; }
        public int LevelNumber => levels.LevelNumber;
        public LevelState State { get; private set; }

        public void Start()
        {
            board = BoardFactory.Create(Level);
            blockMaterials = new ColorMaterials(art.BlockMaterial, palette);
            doorMaterials = new ColorMaterials(art.DoorMaterial, palette);
            boardView.Build(board, art, doorMaterials, config.CellSize);

            foreach (var block in board.Blocks)
            {
                var view = Object.Instantiate(config.BlockViewPrefab, boardView.transform);
                view.name = $"Block {block.Id}";
                view.Initialize(block, boardView, config, art, blockMaterials.Get(block.Color));
                view.Exited += OnBlockExited;
                views.Add(view);
            }

            boardCamera.Frame(boardView.WorldBounds);
            drag.Attach(board, views);
            drag.BlockMoved += OnBlockMoved;
            drag.BlockLeft += OnBlockLeft;

            SetState(LevelState.Playing);
            CheckStuck();
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
            blockMaterials?.Dispose();
            doorMaterials?.Dispose();
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
            var (isCanceled, isSolved) = await autoPlayer.PlayAsync(board, views, OnBlockLeft, lifetime.Token)
                .SuppressCancellationThrow();
            if (isCanceled || isSolved)
            {
                return;
            }

            // The solver gave up within its budget; hand the board back to the player.
            Debug.LogWarning("Auto play found no solution from this board.");
            SetState(LevelState.Playing);
            CheckStuck();
        }

        private void OnBlockMoved(BoardBlock block)
        {
            CheckStuck();
        }

        private void OnBlockLeft(BoardBlock block, BoardDoor door)
        {
            if (board.IsCleared)
            {
                Win();
                return;
            }

            CheckStuck();
        }

        private void OnBlockExited(BlockView view)
        {
            bursts.Play(view.Center, palette.GetColor(view.Block.Color));
            haptics.Play();
        }

        private void CheckStuck()
        {
            if (State != LevelState.Playing)
            {
                return;
            }

            if (solver.Solve(board, config.StuckSearchBudget).IsStuck)
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

            // A level tried from the level editor is not part of the player's progress.
            if (!levels.IsEditorTest)
            {
                progression.CompleteCurrentLevel();
            }

            var reward = economy.LevelCompleteReward;
            wallet.Add(reward);
            outcome.Win(reward);
            ShowResultAsync<LevelCompletePopup>().Forget();
        }

        private void Fail(LevelFailReason reason)
        {
            if (State is not (LevelState.Playing or LevelState.AutoPlaying))
            {
                return;
            }

            SetState(LevelState.Failed);
            outcome.Fail(reason);
            ShowResultAsync<LevelFailPopup>().Forget();
        }

        private async UniTaskVoid ShowResultAsync<TPopup>() where TPopup : Popup
        {
            var isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(config.ResultPopupDelay), cancellationToken: lifetime.Token)
                .SuppressCancellationThrow();
            if (!isCanceled)
            {
                popups.ShowAsync<TPopup>(lifetime.Token).Forget();
            }
        }

        private void SetState(LevelState state)
        {
            State = state;
            drag.IsEnabled = state == LevelState.Playing;
            StateChanged?.Invoke();
        }
    }
}
