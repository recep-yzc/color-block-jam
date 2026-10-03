using System.Diagnostics;

namespace ColorBlockJam.Level
{
    public static class EditorTestLevel
    {
        private const string SessionKey = "ColorBlockJam.EditorTestLevel";

        public static bool IsActive => !string.IsNullOrEmpty(Json);

        private static string Json
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.SessionState.GetString(SessionKey, string.Empty);
#else
                return string.Empty;
#endif
            }
        }

        public static bool TryGet(out LevelData level)
        {
            var json = Json;
            level = string.IsNullOrEmpty(json) ? null : LevelSerializer.FromJson(json);
            return level != null;
        }

        [Conditional("UNITY_EDITOR")]
        public static void Begin(LevelData level)
        {
#if UNITY_EDITOR
            UnityEditor.SessionState.SetString(SessionKey, LevelSerializer.ToJson(level));
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void End()
        {
#if UNITY_EDITOR
            UnityEditor.SessionState.EraseString(SessionKey);
#endif
        }
    }
}
