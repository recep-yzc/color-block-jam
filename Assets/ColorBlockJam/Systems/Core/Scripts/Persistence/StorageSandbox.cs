using System.Diagnostics;

namespace ColorBlockJam.Core.Persistence
{
    public static class StorageSandbox
    {
        private const string SessionKey = "ColorBlockJam.StorageSandbox";

        public static bool IsActive
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.SessionState.GetBool(SessionKey, false);
#else
                return false;
#endif
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void Begin()
        {
#if UNITY_EDITOR
            UnityEditor.SessionState.SetBool(SessionKey, true);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void End()
        {
#if UNITY_EDITOR
            UnityEditor.SessionState.EraseBool(SessionKey);
#endif
        }
    }
}
