using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay
{
    public static class EditorTestLevel
    {
        public const string SessionKey = "ColorBlockJam.EditorTestLevel";

        public static bool TryGet(out LevelData level)
        {
#if UNITY_EDITOR
            var json = UnityEditor.SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                level = LevelSerializer.FromJson(json);
                return true;
            }
#endif
            level = null;
            return false;
        }
    }
}
