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
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class BlockView : MonoBehaviour
    {
        private const float MinMotionDuration = 0.05f;
        private const float LineUpShare = 0.6f;
        private const float LeftScale = 0.2f;
        private const float SmashSpread = 1.2f;
        private const float SmashHeight = 0.3f;
        private const int ShakeFrequency = 6;
        private const float IceCountLift = 0.25f;

        [Header("Ice")]
        [Tooltip("Donmuş bloğun etrafındaki buz kabuğu. Bloğun mesh'ini ve buz materyalini alır. Prefab'da kapalı " +
                 "durur, sadece donmuş bloklarda açılır.")]
        [SerializeField] private MeshFilter iceShell;
        [Tooltip("Buzun kırılması için kaç blok daha çıkması gerektiğini gösteren sayı. Kabuk gibi prefab'da kapalı " +
                 "durur.")]
        [SerializeField] private TMP_Text iceCount;

        private BoardView boardView;
        private GameplayConfig config;
        private Mesh mesh;
        private MeshRenderer meshRenderer;

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

        public event Action<BlockView> Removed;

        public BoardBlock Block { get; private set; }

        public bool IsFrozen => iceLeft > 0;

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

        public void Follow(NVector2 cell)
        {
            MoveTo(new Vector2(cell.X, cell.Y));
        }

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

        public void PlayFrozenShake()
        {
            shakeMotion.TryComplete();
            shakeMotion = LMotion.Punch.Create(0f, config.FrozenShakeStrength, config.FrozenShakeDuration)
                .WithFrequency(ShakeFrequency)
                .Bind(this, static (value, view) => view.SetShake(value))
                .AddTo(this);
        }

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
