using UnityEngine;

namespace ColorBlockJam.Level
{
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
