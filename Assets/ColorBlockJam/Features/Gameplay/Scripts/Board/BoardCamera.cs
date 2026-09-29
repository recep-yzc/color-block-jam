using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Frames the board with an orthographic camera tilted by the configured pitch,
    /// leaving the configured share of the screen to the HUD.
    /// </summary>
    public sealed class BoardCamera
    {
        private const float Distance = 60f;

        private readonly Camera viewCamera;
        private readonly GameplayConfig config;

        public BoardCamera(Camera viewCamera, GameplayConfig config)
        {
            this.viewCamera = viewCamera;
            this.config = config;
        }

        public void Frame(Bounds board)
        {
            var pitch = config.CameraPitch * Mathf.Deg2Rad;
            var screenHeight = board.size.z * Mathf.Sin(pitch) + board.size.y * Mathf.Cos(pitch);
            var halfHeight = screenHeight * 0.5f / config.BoardScreenHeight;
            var halfWidth = board.size.x * 0.5f / config.BoardScreenWidth / viewCamera.aspect;

            viewCamera.orthographic = true;
            viewCamera.orthographicSize = Mathf.Max(halfHeight, halfWidth);
            viewCamera.transform.rotation = Quaternion.Euler(config.CameraPitch, 0f, 0f);
            viewCamera.transform.position = board.center - viewCamera.transform.forward * Distance;
        }
    }
}
