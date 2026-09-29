using System;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The look of one block. While dragged it lifts and follows its logical place with a soft spring,
    /// so the curved path the drag mover finds around corners reads as smooth motion.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class BlockView : MonoBehaviour
    {
        private BoardView boardView;
        private GameplayConfig config;
        private Mesh mesh;
        private Vector2 cellPosition;
        private Vector2 middle;
        private Vector2 followTarget;
        private float lift;
        private bool isFollowing;
        private MotionHandle liftMotion;
        private MotionHandle moveMotion;

        /// <summary>Raised when the block has slid out through its door, just before it hides.</summary>
        public event Action<BlockView> Exited;

        public BoardBlock Block { get; private set; }

        /// <summary>World position of the middle of the block, at half its height.</summary>
        public Vector3 Center => transform.position + Vector3.up * (config.CellSize * 0.2f * transform.localScale.y / RestScale);

        private float RestScale => config.CellSize / ArtSpace.CellSize;

        public void Initialize(BoardBlock block, BoardView board, GameplayConfig gameplayConfig, BoardArt art, Material material)
        {
            Block = block;
            boardView = board;
            config = gameplayConfig;
            mesh = BlockMeshBuilder.Build(block, art);
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterial = material;
            transform.localScale = Vector3.one * RestScale;
            middle = new Vector2(block.MinX + block.MaxX + 1, block.MinY + block.MaxY + 1) * 0.5f;
            cellPosition = new Vector2(block.Position.X, block.Position.Y);
            Apply();
        }

        public void BeginDrag()
        {
            moveMotion.TryCancel();
            followTarget = cellPosition;
            isFollowing = true;
            AnimateLift(config.LiftHeight);
        }

        /// <summary>Sets where the dragged block is on the board, in cell units.</summary>
        public void Follow(System.Numerics.Vector2 cell)
        {
            followTarget = new Vector2(cell.X, cell.Y);
        }

        /// <summary>Ends a drag by settling on a board cell.</summary>
        public void Settle(GridPoint cell)
        {
            isFollowing = false;
            AnimateLift(0f);
            moveMotion.TryCancel();
            moveMotion = LMotion.Create(cellPosition, new Vector2(cell.X, cell.Y), config.SnapDuration)
                .WithEase(config.SnapEase)
                .Bind(this, static (position, view) => view.MoveTo(position))
                .AddTo(this);
        }

        /// <summary>Slides straight to a cell, as auto play moves.</summary>
        public UniTask SlideAsync(GridPoint cell, CancellationToken cancellationToken)
        {
            isFollowing = false;
            moveMotion.TryCancel();
            var target = new Vector2(cell.X, cell.Y);
            var duration = Mathf.Max(0.05f, Vector2.Distance(cellPosition, target) * config.AutoPlayCellDuration);
            moveMotion = LMotion.Create(cellPosition, target, duration)
                .WithEase(Ease.OutCubic)
                .Bind(this, static (position, view) => view.MoveTo(position))
                .AddTo(this);
            return moveMotion.ToUniTask(cancellationToken);
        }

        /// <summary>Slides out through a door, shrinking, then hides.</summary>
        public async UniTask ExitAsync(Direction direction, float distance, CancellationToken cancellationToken)
        {
            isFollowing = false;
            moveMotion.TryCancel();
            AnimateLift(0f);

            var offset = direction.ToOffset();
            var target = cellPosition + new Vector2(offset.X, offset.Y) * distance;
            var duration = Mathf.Max(0.05f, distance / config.ExitSpeed);
            var restScale = transform.localScale;

            moveMotion = LMotion.Create(cellPosition, target, duration)
                .WithEase(Ease.InQuad)
                .Bind(this, static (position, view) => view.MoveTo(position))
                .AddTo(this);
            var shrink = LMotion.Create(restScale, restScale * 0.2f, duration)
                .WithEase(Ease.InCubic)
                .BindToLocalScale(transform)
                .AddTo(this);

            await UniTask.WhenAll(moveMotion.ToUniTask(cancellationToken), shrink.ToUniTask(cancellationToken));
            Exited?.Invoke(this);
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!isFollowing)
            {
                return;
            }

            var blend = 1f - Mathf.Exp(-config.FollowSharpness * Time.deltaTime);
            MoveTo(Vector2.Lerp(cellPosition, followTarget, blend));
        }

        private void OnDestroy()
        {
            Destroy(mesh);
        }

        private void AnimateLift(float height)
        {
            liftMotion.TryCancel();
            liftMotion = LMotion.Create(lift, height, config.LiftDuration)
                .WithEase(Ease.OutQuad)
                .Bind(this, static (value, view) => view.SetLift(value))
                .AddTo(this);
        }

        private void MoveTo(Vector2 cell)
        {
            cellPosition = cell;
            Apply();
        }

        private void SetLift(float value)
        {
            lift = value;
            Apply();
        }

        private void Apply()
        {
            transform.position = boardView.CellToWorld(cellPosition + middle) + Vector3.up * lift;
        }
    }
}
