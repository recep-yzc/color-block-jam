using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;

namespace ColorBlockJam.Tests
{
    internal static class TestAssets
    {
        public static T LoadOnly<T>() where T : UnityEngine.Object
        {
            var all = LoadAll<T>();
            Assert.AreEqual(1, all.Count, $"Expected one {typeof(T).Name}.");
            return all[0];
        }

        public static List<T> LoadAll<T>() where T : UnityEngine.Object
        {
            var assets = new List<T>();
            foreach (var guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                assets.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            return assets;
        }
    }
}
