using System;
using System.Threading;
using ColorBlockJam.Gameplay.Logic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using NVector2 = System.Numerics.Vector2;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The look of one block. While dragged it lifts, gets an outline and sits exactly where the drag mover puts it,
    /// so what the player sees never overlaps another block; the smoothness comes from how the drag catches up with
    /// the finger. A frozen block wears a shell of ice showing how many blocks still have to leave.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class BlockView : MonoBehaviour
    {
        // Shortest time any move of the block takes, so tiny moves still read as motion.
        private const float MinMotionDuration = 0.05f;

        // Lining up in front of a door takes this share of the snap duration.
        private const float LineUpShare = 0.6f;

        // Scale a leaving block shrinks to while it slides out.
        private const float LeftScale = 0.2f;

        // Shape a block squashes to under the hammer, as shares of its size.
        private const float SmashSpread = 1.2f;
        private const float SmashHeight = 0.3f;

        // Swings of the shake a frozen block gives when it is pulled.
        private const int ShakeFrequency = 6;

        // Gap, in art units, between the top of the block and its ice count.
        private const float IceCountLift = 0.25f;

        [Header("Ice")]
        [Tooltip("Shell drawn around a frozen block; it gets the block's mesh and the ice material. Kept inactive in " +
                 "the prefab and shown only on frozen blocks.")]
        [SerializeField] private MeshFilter iceShell;
        [Tooltip("How many more blocks must leave before the ice breaks. Kept inactive in the prefab like the shell.")]
        [SerializeField] private TMP_Text iceCount;

        private BoardView boardView;
        private GameplayConfig config;
        private Mesh mesh;
        private MeshRenderer meshRenderer;

        // The material sets for resting and for being held, made once so a grab allocates nothing.
        private Material[] restMaterials;
        private Material[] heldMaterials;
        private Vector2 cellPosition;
        private Vector2 middle;
        private float lift;
        private float shake;
        private int iceLeft;
        private MotionHandle liftMotion;
        private MotionHandle moveMotion;
        private MotionHandle shakeMotion;
        private MotionHandle iceMotion;

        /// <summary>
        /// Raised when the block is gone from the board, out through its door or broken by the hammer, just before
        /// it hides.
        /// </summary>
        public event Action<BlockView> Removed;

        public BoardBlock Block { get; private set; }

        /// <summary>True while the block wears its ice.</summary>
        public bool IsFrozen => iceLeft > 0;

        /// <summary>World position of the middle of the block, at half its height.</summary>
        public Vector3 Center => transform.position + Vector3.up * (config.CellSize * ArtSpace.BlockHalfHeight * transform.localScale.y / RestScale);

        private float RestScale => config.CellSize / ArtSpace.CellSize;

        public void Initialize(BoardBlock block, BoardView board, GameplayConfig gameplayConfig, BoardArt art, Color color)
        {
            Block = block;
            boardView = board;
            config = gameplayConfig;
            mesh = BlockMeshBuilder.Build(block, art, color);
            GetComponent<MeshFilter>().sharedMesh = mesh;
            meshRenderer = GetComponent<MeshRenderer>();
            restMaterials = new[] { art.BlockMaterial };
            heldMaterials = art.BlockOutlineMaskMaterial != null && art.BlockOutlineMaterial != null
                ? new[] { art.BlockMaterial, art.BlockOutlineMaskMaterial, art.BlockOutlineMaterial }
                : restMaterials;
            meshRenderer.sharedMaterials = restMaterials;
            transform.localScale = Vector3.one * RestScale;
            middle = new Vector2(block.MinX + block.MaxX + 1, block.MinY + block.MaxY + 1) * 0.5f;
            cellPosition = new Vector2(block.Position.X, block.Position.Y);

            iceLeft = block.Ice;
            if (IsFrozen)
            {
                iceShell.sharedMesh = mesh;
                iceShell.GetComponent<MeshRenderer>().sharedMaterial = art.IceMaterial;
                iceShell.gameObject.SetActive(true);
                iceCount.transform.localPosition = IceCountPosition(block);
                iceCount.SetText("{0}", iceLeft);
                iceCount.gameObject.SetActive(true);
            }

            Apply();
        }

        public void BeginDrag()
        {
            moveMotion.TryCancel();
            meshRenderer.sharedMaterials = heldMaterials;
            AnimateLift(config.LiftHeight);
        }

        /// <summary>Puts the dragged block where it is on the board, in cell units.</summary>
        public void Follow(NVector2 cell)
        {
            MoveTo(new Vector2(cell.X, cell.Y));
        }

        /// <summary>Ends a drag by settling on a board cell.</summary>
        public void Settle(GridPoint cell)
        {
            meshRenderer.sharedMaterials = restMaterials;
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
            moveMotion.TryCancel();
            var target = new Vector2(cell.X, cell.Y);
            var duration = Mathf.Max(MinMotionDuration, Vector2.Distance(cellPosition, target) * config.AutoPlayCellDuration);
            moveMotion = LMotion.Create(cellPosition, target, duration)
                .WithEase(Ease.OutCubic)
                .Bind(this, static (position, view) => view.MoveTo(position))
                .AddTo(this);
            return moveMotion.ToUniTask(cancellationToken);
        }

        /// <summary>
        /// Lines up with <paramref name="cell"/> in front of a door, then slides out through it, which takes
        /// <paramref name="stepsToLeave"/> cells from there.
        /// </summary>
        public async UniTask LeaveFromAsync(GridPoint cell, Direction direction, float stepsToLeave, CancellationToken cancellationToken)
        {
            meshRenderer.sharedMaterials = restMaterials;
            moveMotion.TryCancel();
            moveMotion = LMotion.Create(cellPosition, new Vector2(cell.X, cell.Y), config.SnapDuration * LineUpShare)
                .WithEase(Ease.OutQuad)
                .Bind(this, static (position, view) => view.MoveTo(position))
                .AddTo(this);
            await moveMotion.ToUniTask(cancellationToken);

            await ExitAsync(direction, stepsToLeave, cancellationToken);
        }

        /// <summary>
        /// Slides out through a door, shrinking, then hides. <paramref name="stepsToLeave"/> is how many cells take it
        /// fully off the board; it slides a little further so it clears the wall.
        /// </summary>
        public async UniTask ExitAsync(Direction direction, float stepsToLeave, CancellationToken cancellationToken)
        {
            var distance = stepsToLeave + config.ExitOvershoot;
            moveMotion.TryCancel();
            meshRenderer.sharedMaterials = restMaterials;
            AnimateLift(0f);

            var offset = direction.ToOffset();
            var target = cellPosition + new Vector2(offset.X, offset.Y) * distance;
            var duration = Mathf.Max(MinMotionDuration, distance / config.ExitSpeed);
            var restScale = transform.localScale;

            moveMotion = LMotion.Create(cellPosition, target, duration)
                .WithEase(Ease.InQuad)
                .Bind(this, static (position, view) => view.MoveTo(position))
                .AddTo(this);
            var shrink = LMotion.Create(restScale, restScale * LeftScale, duration)
                .WithEase(Ease.InCubic)
                .BindToLocalScale(transform)
                .AddTo(this);

            await UniTask.WhenAll(moveMotion.ToUniTask(cancellationToken), shrink.ToUniTask(cancellationToken));
            Removed?.Invoke(this);
            gameObject.SetActive(false);
        }

        /// <summary>Breaks the block where it stands, as the hammer does: it squashes flat, then pops and hides.</summary>
        public async UniTask SmashAsync(CancellationToken cancellationToken)
        {
            moveMotion.TryCancel();
            var restScale = transform.localScale;
            var squashed = new Vector3(restScale.x * SmashSpread, restScale.y * SmashHeight, restScale.z * SmashSpread);
            moveMotion = LMotion.Create(restScale, squashed, config.SmashDuration)
                .WithEase(Ease.InBack)
                .BindToLocalScale(transform)
                .AddTo(this);
            await moveMotion.ToUniTask(cancellationToken);

            Removed?.Invoke(this);
            gameObject.SetActive(false);
        }

        /// <summary>A short shake, when the player tries to move the block while it is frozen.</summary>
        public void PlayFrozenShake()
        {
            shakeMotion.TryComplete();
            shakeMotion = LMotion.Punch.Create(0f, config.FrozenShakeStrength, config.FrozenShakeDuration)
                .WithFrequency(ShakeFrequency)
                .Bind(this, static (value, view) => view.SetShake(value))
                .AddTo(this);
        }

        /// <summary>Shows how many more blocks must leave before this one thaws; at zero the ice breaks away.</summary>
        public void ShowIce(int left)
        {
            if (!IsFrozen || left == iceLeft)
            {
                return;
            }

            iceLeft = left;
            iceMotion.TryComplete();
            if (IsFrozen)
            {
                iceCount.SetText("{0}", left);
                iceMotion = LMotion.Punch.Create(Vector3.one, Vector3.one * config.IceCountPunch, config.IceBreakDuration)
                    .BindToLocalScale(iceCount.transform)
                    .AddTo(this);
                return;
            }

            iceCount.gameObject.SetActive(false);
            iceMotion = LMotion.Create(Vector3.one, Vector3.zero, config.IceBreakDuration)
                .WithEase(Ease.InBack)
                .WithOnComplete(HideIce)
                .BindToLocalScale(iceShell.transform)
                .AddTo(this);
        }

        private void OnDestroy()
        {
            Destroy(mesh);
        }

        /// <summary>Just over the top of the cell <see cref="BlockMarks"/> picks for the count.</summary>
        private Vector3 IceCountPosition(BoardBlock block)
        {
            var cell = BlockMarks.FindIceCell(block.Cells);
            var local = (new Vector2(cell.X + 0.5f, cell.Y + 0.5f) - middle) * ArtSpace.CellSize;
            return new Vector3(local.x, mesh.bounds.max.y + IceCountLift, local.y);
        }

        private void HideIce()
        {
            iceShell.gameObject.SetActive(false);
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

        private void SetShake(float value)
        {
            shake = value;
            Apply();
        }

        private void Apply()
        {
            transform.position = boardView.CellToWorld(cellPosition + middle + new Vector2(shake, 0f)) + Vector3.up * lift;
        }
    }
}
