using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using Framework.Settings;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The level on screen. It builds the board's rules and views from the level data and frames the camera, then
    /// plays what the player sees when a block leaves: the door opening, the burst in the block's color and a haptic.
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
                view.Exited += OnBlockExited;
                views.Add(view);
            }

            boardCamera.Frame(boardView.WorldBounds);
        }

        /// <summary>Opens the door a block is leaving through.</summary>
        public void PlayLeave(BoardDoor door)
        {
            if (door != null)
            {
                boardView.PlayDoorEntry(door);
            }
        }

        private void OnBlockExited(BlockView view)
        {
            bursts.Play(view.Center, palette.GetColor(view.Block.Color));
            haptics.Play();
        }
    }
}
