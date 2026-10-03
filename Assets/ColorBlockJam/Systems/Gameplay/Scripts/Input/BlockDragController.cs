using System;
using ColorBlockJam.Gameplay.Logic;
using UnityEngine;
using VContainer.Unity;
using NVector2 = System.Numerics.Vector2;

namespace ColorBlockJam.Gameplay
{
    public sealed class BlockDragController : ITickable
    {
        private const float MinChase = 0.0005f;

        private readonly BoardPointer pointer;
        private readonly BoardView boardView;
        private readonly Camera viewCamera;
        private readonly GameplayConfig config;
        private readonly BlockPressRouter pressRouter;
        private readonly BlockDragMover mover;
        private readonly LevelBoard levelBoard;

        private BlockView dragged;
        private NVector2 position;
        private NVector2 target;
        private NVector2 grabOffset;

        public BlockDragController(BoardPointer pointer, BoardView boardView, Camera viewCamera, GameplayConfig config,
            BlockPressRouter pressRouter, LevelBoard levelBoard)
        {
            this.pointer = pointer;
            this.boardView = boardView;
            this.viewCamera = viewCamera;
            this.config = config;
            this.pressRouter = pressRouter;
            this.levelBoard = levelBoard;
            mover = new BlockDragMover(config.CornerRounding);
        }

        public event Action BlockPressed;

        public event Action BlockMoved;

        public bool IsEnabled { get; set; }

        private float PickHeight => config.CellSize * ArtSpace.BlockHalfHeight;

        private Board Board => levelBoard.Board;

        public void Tick()
        {
            if (Board == null)
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
                Release(canEnterDoor: IsEnabled);
            }
        }

        private void TryGrab()
        {
            if (!TryGetPointerCell(out var cell))
            {
                return;
            }

            var block = BlockPicker.Pick(Board, cell, config.PickPadding, out var pressed);
            if (block == null)
            {
                return;
            }

            BlockPressed?.Invoke();
            if (pressRouter.TryPick(block, pressed))
            {
                return;
            }

            if (Board.IsFrozen(block))
            {
                levelBoard.Views[block.Id].PlayFrozenShake();
                return;
            }

            dragged = levelBoard.Views[block.Id];
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
                position = mover.Move(Board, block, position, position + toTarget * (step / distance));
            }

            var depth = BlockPlacement.DepthThroughDoor(Board, block, position, out var door);
            if (door != null && depth >= config.ExitDepth &&
                Board.CanPassThrough(block, BlockPlacement.EdgePosition(Board, block, position, door.Side), door.ExitDirection))
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
            var cell = BlockPlacement.Snap(Board, block, position);

            Board.Move(block, cell);
            dragged = null;

            var door = canEnterDoor ? BlockPlacement.DoorToEnter(Board, block, cell) : null;
            if (door != null)
            {
                levelBoard.LeaveFrom(block, cell, door.ExitDirection);
                return;
            }

            view.Settle(cell);
            if (cell != start)
            {
                BlockMoved?.Invoke();
            }
        }

        private void Leave(BoardDoor door, float depth)
        {
            var block = dragged.Block;
            dragged = null;
            levelBoard.Exit(block, door, depth);
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
