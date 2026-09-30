using ColorBlockJam.Boosters;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Tests
{
    internal static class TestBoosters
    {
        public static T Create<T>(string id, int unlockLevel, int startingCount, int coinCost) where T : BoosterDefinition
        {
            var booster = ScriptableObject.CreateInstance<T>();
            var fields = new SerializedObject(booster);
            fields.FindProperty("id").stringValue = id;
            fields.FindProperty("unlockLevel").intValue = unlockLevel;
            fields.FindProperty("startingCount").intValue = startingCount;
            fields.FindProperty("coinCost").intValue = coinCost;
            fields.ApplyModifiedPropertiesWithoutUndo();
            return booster;
        }
    }
}
