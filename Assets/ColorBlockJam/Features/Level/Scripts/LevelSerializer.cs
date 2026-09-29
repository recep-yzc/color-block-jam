using UnityEngine;

namespace ColorBlockJam.Level
{
    /// <summary>
    /// The one level format: JSON of <see cref="LevelData"/>. Used by the game and by the level editor.
    /// </summary>
    public static class LevelSerializer
    {
        public static string ToJson(LevelData level)
        {
            return JsonUtility.ToJson(level, prettyPrint: true);
        }

        public static LevelData FromJson(string json)
        {
            return JsonUtility.FromJson<LevelData>(json);
        }
    }
}
