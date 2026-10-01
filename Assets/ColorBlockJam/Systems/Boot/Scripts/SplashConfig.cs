using UnityEngine;

namespace ColorBlockJam.Boot
{
    [CreateAssetMenu(menuName = "Color Block Jam/Boot/Splash Config", fileName = "SplashConfig")]
    public sealed class SplashConfig : ScriptableObject
    {
        [Tooltip("Açılış ekranından sonra açılan sahne. Build ayarlarında olmalı.")]
        [SerializeField] private string nextScene;

        [Tooltip("Yükleme daha hızlı bitse bile açılış ekranının en az açık kalacağı süre, saniye.")]
        [SerializeField, Min(0f)] private float minimumDuration = 1.5f;

        [Tooltip("Yükleme çubuğunun dolma hızı, saniyede tam çubuk.")]
        [SerializeField, Min(0.1f)] private float barFillSpeed = 1.5f;

        [Tooltip("Yükleme çubuğunun başlangıç işlerine ayrılan kısmı. Kalanı sahne yüklemesine gider.")]
        [SerializeField, Range(0f, 1f)] private float startupTasksShare = 0.3f;

        public string NextScene => nextScene;
        public float MinimumDuration => minimumDuration;
        public float BarFillSpeed => barFillSpeed;
        public float StartupTasksShare => startupTasksShare;
    }
}
