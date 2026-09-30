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
    /// Turns pointer input into block moves: grab the block under the finger, settle it on the nearest free cell on
    /// release, or let it leave when it is pushed through its door. A block dropped right in front of a door it fits
    /// through goes in on its own. A frozen block only shakes.
    /// The finger only sets a target. Every frame the block catches up with it like a weight on a spring, fast when far
    /// and gently when close, and the drag mover sweeps that step so the block slides along and rolls around whatever
    /// is in the way. It keeps catching up while the finger rests, so a quick flick is never left halfway.
    /// </summary>
    public sealed class BlockDragController : ITickable
    {
        // Closer to the finger than this, in cells, the block is where it should be.
        private const float MinChase = 0.0005f;

        private readonly BoardPointer pointer;
        private readonly BoardView boardView;
        private readonly Camera viewCamera;
        private readonly GameplayConfig config;
        private readonly BlockDragMover mover;

        private Board board;
        private IReadOnlyList<BlockView> views;
        private BlockView dragged;
        private NVector2 position;
        private NVector2 target;
        private NVector2 grabOffset;

        public BlockDragController(BoardPointer pointer, BoardView boardView, Camera viewCamera, GameplayConfig config)
        {
            this.pointer = pointer;
            this.boardView = boardView;
            this.viewCamera = viewCamera;
            this.config = config;
            mover = new BlockDragMover(config.CornerRounding);
        }

        /// <summary>Raised when a dragged block settles on a different cell.</summary>
        public event Action<BoardBlock> BlockMoved;

        /// <summary>Raised when a block is pushed out through its door.</summary>
        public event Action<BoardBlock, BoardDoor> BlockLeft;

        public bool IsEnabled { get; set; }

        // Blocks are picked at half their height so taps hit what the player sees.
        private float PickHeight => config.CellSize * ArtSpace.BlockHalfHeight;

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
                if (IsEnabled && pointer.WasPressedThisFrame && !pointer.IsOverUI)
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
                // A drag ended by a pause or the end of the level only settles the block.
                Release(canEnterDoor: IsEnabled);
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

            if (board.IsFrozen(block))
            {
                views[block.Id].PlayFrozenShake();
                return;
            }

            dragged = views[block.Id];
            position = new NVector2(block.Position.X, block.Position.Y);
            target = position;
            grabOffset = cell - position;
            dragged.BeginDrag();
        }

        private void Drag()
        {
            if (TryGetPointerCell(out var cell))
            {
                target = cell - grabOffset;
            }

            var block = dragged.Block;
            var toTarget = target - position;
            var distance = toTarget.Length();
            if (distance > MinChase)
            {
                var deltaTime = Time.deltaTime;
                var step = MathF.Min(distance * (1f - MathF.Exp(-config.FollowSharpness * deltaTime)), config.MaxDragSpeed * deltaTime);
                position = mover.Move(board, block, position, position + toTarget * (step / distance));
            }

            var depth = BlockPlacement.DepthThroughDoor(board, block, position, out var door);
            if (door != null && depth >= config.ExitDepth &&
                board.CanPassThrough(block, BlockPlacement.EdgePosition(board, block, position, door.Side), door.ExitDirection))
            {
                Leave(door, depth);
                return;
            }

            dragged.Follow(position);
        }

        private void Release(bool canEnterDoor)
        {
            var block = dragged.Block;
            var view = dragged;
            var start = block.Position;
            var cell = BlockPlacement.Snap(board, block, position);

            board.Move(block, cell);
            dragged = null;

            var door = canEnterDoor ? BlockPlacement.DoorToEnter(board, block, cell) : null;
            if (door != null)
            {
                var steps = BlockPlacement.StepsToLeave(board, block, cell, door.ExitDirection);
                board.Clear(block);
                view.LeaveFromAsync(cell, door.ExitDirection, steps, view.destroyCancellationToken).Forget();
                BlockLeft?.Invoke(block, door);
                return;
            }

            view.Settle(cell);
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
            var distance = BlockPlacement.LengthThroughDoor(block, door.Side) - depth;
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
