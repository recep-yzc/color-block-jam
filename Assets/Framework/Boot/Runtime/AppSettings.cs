using UnityEngine;

namespace Framework.Boot
{
    [CreateAssetMenu(menuName = "Framework/Boot/App Settings", fileName = "AppSettings")]
    public sealed class AppSettings : ScriptableObject
    {
        [Tooltip("Uygulamanın hedef kare hızı.")]
        [SerializeField, Min(30)] private int targetFrameRate = 60;
        [Tooltip("Açıkken uygulama çalışırken ekran kararmaz.")]
        [SerializeField] private bool preventScreenSleep = true;

        public int TargetFrameRate => targetFrameRate;
        public bool PreventScreenSleep => preventScreenSleep;
    }
}
