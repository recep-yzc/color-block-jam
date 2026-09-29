using System;
using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using NVector2 = System.Numerics.Vector2;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Turns pointer input into block moves: grab the block under the finger, move it with the drag mover
    /// every frame, settle it on the nearest free cell on release, or let it leave when it is pushed through its door.
    /// </summary>
    public sealed class BlockDragController : ITickable
    {
        private readonly BoardPointer pointer;
        private readonly BoardView boardView;
        private readonly Camera viewCamera;
        private readonly GameplayConfig config;
        private readonly BlockDragMover mover;

        private Board board;
        private IReadOnlyList<BlockView> views;
        private BlockView dragged;
        private NVector2 position;
        private NVector2 grabOffset;

        public BlockDragController(BoardPointer pointer, BoardView boardView, Camera viewCamera, GameplayConfig config)
        {
            this.pointer = pointer;
            this.boardView = boardView;
            this.viewCamera = viewCamera;
            this.config = config;
            mover = new BlockDragMover(config.CornerAssist, config.CornerAssistRate);
        }

        /// <summary>Raised when a dragged block settles on a different cell.</summary>
        public event Action<BoardBlock> BlockMoved;

        /// <summary>Raised when a block is pushed out through its door.</summary>
        public event Action<BoardBlock, BoardDoor> BlockLeft;

        public bool IsEnabled { get; set; }

        // Blocks are picked at half their height so taps hit what the player sees.
        private float PickHeight => config.CellSize * 0.4f;

        public void Attach(Board targetBoard, IReadOnlyList<BlockView> blockViews)
        {
            board = targetBoard;
            views = blockViews;
        }

        public void Tick()
        {
            if (board == null)
            {
                return;
            }

            if (dragged == null)
            {
                if (IsEnabled && pointer.WasPressedThisFrame && !pointer.IsOverUi)
                {
                    TryGrab();
                }

                return;
            }

            if (IsEnabled && pointer.IsPressed)
            {
                Drag();
            }
            else
            {
                Release();
            }
        }

        private void TryGrab()
        {
            if (!TryGetPointerCell(out var cell))
            {
                return;
            }

            var block = board.BlockAt(new GridPoint((int)MathF.Floor(cell.X), (int)MathF.Floor(cell.Y)));
            if (block == null)
            {
                return;
            }

            dragged = views[block.Id];
            position = new NVector2(block.Position.X, block.Position.Y);
            grabOffset = cell - position;
            dragged.BeginDrag();
        }

        private void Drag()
        {
            if (!TryGetPointerCell(out var cell))
            {
                return;
            }

            var block = dragged.Block;
            position = mover.Move(board, block, position, cell - grabOffset);

            var depth = BlockPlacement.DepthThroughDoor(board, block, position, out var door);
            if (door != null && depth >= config.ExitDepth &&
                board.CanPassThrough(block, BlockPlacement.EdgePosition(board, block, position, door.Side), door.ExitDirection))
            {
                Leave(door, depth);
                return;
            }

            dragged.Follow(position);
        }

        private void Release()
        {
            var block = dragged.Block;
            var start = block.Position;
            var cell = BlockPlacement.Snap(board, block, position);

            board.Move(block, cell);
            dragged.Settle(cell);
            dragged = null;

            if (cell != start)
            {
                BlockMoved?.Invoke(block);
            }
        }

        private void Leave(BoardDoor door, float depth)
        {
            var block = dragged.Block;
            var view = dragged;
            dragged = null;

            board.Clear(block);
            var distance = BlockPlacement.LengthThroughDoor(block, door.Side) - depth + 0.5f;
            view.ExitAsync(door.ExitDirection, distance, view.destroyCancellationToken).Forget();
            BlockLeft?.Invoke(block, door);
        }

        private bool TryGetPointerCell(out NVector2 cell)
        {
            if (!boardView.TryScreenToCell(viewCamera, pointer.ScreenPosition, PickHeight, out var unityCell))
            {
                cell = default;
                return false;
            }

            cell = new NVector2(unityCell.x, unityCell.y);
            return true;
        }
    }
}
