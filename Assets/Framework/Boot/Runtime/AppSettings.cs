using UnityEngine;

namespace Framework.Boot
{
    [CreateAssetMenu(menuName = "Framework/Boot/App Settings", fileName = "AppSettings")]
    public sealed class AppSettings : ScriptableObject
    {
        [SerializeField, Min(30)] private int targetFrameRate = 60;
        [SerializeField] private bool preventScreenSleep = true;

        public int TargetFrameRate => targetFrameRate;
        public bool PreventScreenSleep => preventScreenSleep;
    }
}
