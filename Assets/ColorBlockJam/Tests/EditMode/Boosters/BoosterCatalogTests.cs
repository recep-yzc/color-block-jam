using System.Collections.Generic;
using ColorBlockJam.Boosters;
using Framework.UI.Popups;
using NUnit.Framework;
using UnityEditor;

namespace ColorBlockJam.Tests
{
    public sealed class BoosterCatalogTests
    {
        [Test]
        public void EveryShippedBoosterIsComplete()
        {
            var catalog = LoadOnly<BoosterCatalog>();
            Assert.IsNotEmpty(catalog.Boosters);

            var ids = new HashSet<string>();
            foreach (var booster in catalog.Boosters)
            {
                Assert.IsNotNull(booster, "The catalog has an empty slot.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(booster.Id), $"{booster.name} has no id.");
                Assert.IsTrue(ids.Add(booster.Id), $"The id {booster.Id} is used twice, and saves are kept by id.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(booster.DisplayName), $"{booster.name} has no name.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(booster.Description), $"{booster.name} has no description.");
                Assert.IsNotNull(booster.Icon, $"{booster.name} has no icon.");
                Assert.GreaterOrEqual(booster.UnlockLevel, 1, $"{booster.name} unlocks before the first level.");
                Assert.GreaterOrEqual(booster.CoinCost, 1, $"{booster.name} is free.");
                if (booster is AimedBoosterDefinition aimed)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(aimed.AimHint), $"{booster.name} tells the player nothing while aiming.");
                }
            }
        }

        [Test]
        public void TheGameplaySceneCanShowTheUnlockPopup()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PopupCatalog>("Assets/ColorBlockJam/Features/Gameplay/Data/GameplayPopupCatalog.asset");
            Assert.IsNotNull(catalog);

            Assert.IsNotNull(catalog.GetPrefab(typeof(BoosterUnlockPopup)));
        }

        private static T LoadOnly<T>() where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            Assert.AreEqual(1, guids.Length, $"Expected one {typeof(T).Name}.");
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
