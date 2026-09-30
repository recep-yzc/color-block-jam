using UnityEngine;

namespace Framework.Boot
{
    [CreateAssetMenu(menuName = "Framework/Boot/Splash Config", fileName = "SplashConfig")]
    public sealed class SplashConfig : ScriptableObject
    {
        [Tooltip("The scene that opens after the splash screen. It must be in the build settings.")]
        [SerializeField] private string nextScene;

        [Tooltip("The splash screen stays open at least this long, even if loading is faster.")]
        [SerializeField, Min(0f)] private float minimumDuration = 1.5f;

        [Tooltip("How fast the loading bar fills, in full bars per second.")]
        [SerializeField, Min(0.1f)] private float barFillSpeed = 1.5f;

        [Tooltip("Part of the loading bar used by startup tasks. The rest is used by the scene load.")]
        [SerializeField, Range(0f, 1f)] private float startupTasksShare = 0.3f;

        public string NextScene => nextScene;
        public float MinimumDuration => minimumDuration;
        public float BarFillSpeed => barFillSpeed;
        public float StartupTasksShare => startupTasksShare;
    }
}
