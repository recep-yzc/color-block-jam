using System;
using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using ColorBlockJam.Settings;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelBoard : IDisposable
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

        public event Action<BoardBlock> BlockSmashed;

        public Board Board { get; private set; }

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

        public void Smash(BoardBlock block)
        {
            Board.Clear(block);
            var view = views[block.Id];
            view.SmashAsync(view.destroyCancellationToken).Forget();
            BlockSmashed?.Invoke(block);
        }

        public void Dispose()
        {
            foreach (var view in views)
            {
                if (view != null)
                {
                    view.Removed -= OnBlockRemoved;
                    Object.Destroy(view.gameObject);
                }
            }

            views.Clear();
            boardView.Clear();
        }

        private void OnBlockRemoved(BlockView view)
        {
            bursts.Play(view.Center, palette.GetColor(view.Block.Color));
            haptics.Play();
        }
    }
}
