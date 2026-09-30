using System;
using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using Cysharp.Threading.Tasks;
using Framework.Settings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The level on screen. It builds the board's rules and views from the level data and frames the camera, then
    /// plays what the player sees when a block leaves: the door opening, the burst in the block's color, a haptic and
    /// the ice of other blocks melting a step.
    /// </summary>
    public sealed class LevelBoard
    {
        private readonly BoardView boardView;
        private readonly BoardArt art;
        private readonly BlockPalette palette;
        private readonly GameplayConfig config;
        private readonly BoardCamera boardCamera;
        private readonly BlockBurstEffects bursts;
        private readonly IHapticService haptics;
        private readonly List<BlockView> views = new();

        public LevelBoard(BoardView boardView, BoardArt art, BlockPalette palette, GameplayConfig config, BoardCamera boardCamera,
            BlockBurstEffects bursts, IHapticService haptics)
        {
            this.boardView = boardView;
            this.art = art;
            this.palette = palette;
            this.config = config;
            this.boardCamera = boardCamera;
            this.bursts = bursts;
            this.haptics = haptics;
        }

        /// <summary>Raised when a block has been broken where it stood; it is already off the board.</summary>
        public event Action<BoardBlock> BlockSmashed;

        public Board Board { get; private set; }

        /// <summary>The block views, indexed by block id.</summary>
        public IReadOnlyList<BlockView> Views => views;

        public void Build(LevelData level)
        {
            Board = BoardFactory.Create(level);
            boardView.Build(Board, art, palette, config);

            foreach (var block in Board.Blocks)
            {
                var view = Object.Instantiate(config.BlockViewPrefab, boardView.transform);
                view.name = $"Block {block.Id}";
                view.Initialize(block, boardView, config, art, palette.GetColor(block.Color));
                view.Removed += OnBlockRemoved;
                views.Add(view);
            }

            boardCamera.Frame(boardView.WorldBounds);
        }

        /// <summary>
        /// Shows what follows a block leaving the board: the door it went through opens, when it left through one,
        /// and the ice on the other blocks counts down, breaking where it is done.
        /// </summary>
        public void ShowBlockCleared(BoardDoor door)
        {
            if (door != null)
            {
                boardView.PlayDoorEntry(door);
            }

            foreach (var view in views)
            {
                if (!view.IsFrozen || view.Block.IsCleared)
                {
                    continue;
                }

                var left = Board.IceLeft(view.Block);
                view.ShowIce(left);
                if (left == 0)
                {
                    bursts.Play(view.Center, config.IceBurstColor);
                }
            }
        }

        /// <summary>Breaks a block where it stands, as the hammer does, frozen or not.</summary>
        public void Smash(BoardBlock block)
        {
            Board.Clear(block);
            var view = views[block.Id];
            view.SmashAsync(view.destroyCancellationToken).Forget();
            BlockSmashed?.Invoke(block);
        }

        private void OnBlockRemoved(BlockView view)
        {
            bursts.Play(view.Center, palette.GetColor(view.Block.Color));
            haptics.Play();
        }
    }
}
