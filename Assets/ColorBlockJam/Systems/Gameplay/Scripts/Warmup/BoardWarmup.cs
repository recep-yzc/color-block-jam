using System.Threading;
using ColorBlockJam.Core.Startup;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class BoardWarmup : IStartupTask
    {
        private const int RenderedFrames = 2;
        private const float DepthBeforeSceneCameras = -100f;

        private static readonly Vector3 RigPosition = new(0f, -1000f, 0f);
        private static readonly Vector3 CameraOffset = new(0f, 8f, -6f);
        private static readonly Quaternion LightRotation = Quaternion.Euler(50f, -30f, 0f);

        private readonly BoardArt art;
        private readonly BlockPalette palette;
        private readonly GameplayConfig config;
        private readonly LightShadows mainLightShadows;

        public BoardWarmup(BoardArt art, BlockPalette palette, GameplayConfig config, LightShadows mainLightShadows)
        {
            this.art = art;
            this.palette = palette;
            this.config = config;
            this.mainLightShadows = mainLightShadows;
        }

        public async UniTask RunAsync(CancellationToken cancellationToken)
        {
            var rig = new GameObject("Board Warmup").transform;
            rig.position = RigPosition;
            try
            {
                Build(rig);
                await UniTask.DelayFrame(RenderedFrames, cancellationToken: cancellationToken);
            }
            finally
            {
                Object.Destroy(rig.gameObject);
            }
        }

        private void Build(Transform rig)
        {
            var board = BoardFactory.Create(BoardWarmupLevel.Create());
            var boardView = rig.gameObject.AddComponent<BoardView>();
            boardView.Build(board, art, palette, config);

            foreach (var block in board.Blocks)
            {
                var view = Object.Instantiate(config.BlockViewPrefab, rig);
                view.Initialize(block, boardView, config, art, palette.GetColor(block.Color));
                view.BeginDrag();
            }

            var center = boardView.WorldBounds.center;
            Object.Instantiate(config.BurstPrefab, center, Quaternion.identity, rig).Play(withChildren: true);
            AddLight(rig);
            AddCamera(rig, center);
        }

        private void AddLight(Transform rig)
        {
            var light = new GameObject("Warmup Light").AddComponent<Light>();
            light.transform.SetParent(rig, false);
            light.transform.rotation = LightRotation;
            light.type = LightType.Directional;
            light.shadows = mainLightShadows;
        }

        private static void AddCamera(Transform rig, Vector3 target)
        {
            var camera = new GameObject("Warmup Camera").AddComponent<Camera>();
            camera.transform.SetParent(rig, false);
            camera.transform.position = target + CameraOffset;
            camera.transform.LookAt(target);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.depth = DepthBeforeSceneCameras;
        }
    }
}
