using UnityEngine;

namespace ColorBlockJam.Boot
{
    [CreateAssetMenu(menuName = "Color Block Jam/Boot/App Settings", fileName = "AppSettings")]
    public sealed class AppSettings : ScriptableObject
    {
        [SerializeField, Min(30)] private int targetFrameRate = 60;
        [SerializeField] private bool multiTouchEnabled;
        [SerializeField] private bool preventScreenSleep = true;

        public int TargetFrameRate => targetFrameRate;
        public bool MultiTouchEnabled => multiTouchEnabled;
        public bool PreventScreenSleep => preventScreenSleep;
    }
}
