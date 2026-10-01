using ColorBlockJam.Boosters;
using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Economy;
using NUnit.Framework;
using UnityEngine;

namespace ColorBlockJam.Tests
{
    public sealed class BoosterUnlockRulesTests
    {
        private EconomyConfig economy;
        private BoosterInventory inventory;
        private FreezeBoosterDefinition freeze;
        private HammerBoosterDefinition hammer;
        private RocketBoosterDefinition rocket;
        private BoosterDefinition[] boosters;

        [SetUp]
        public void SetUp()
        {
            var storage = new InMemoryStorage();
            economy = ScriptableObject.CreateInstance<EconomyConfig>();
            inventory = new BoosterInventory(storage, new CoinWallet(storage, economy));
            freeze = TestBoosters.Create<FreezeBoosterDefinition>("freeze", unlockLevel: 2, startingCount: 1, coinCost: 30);
            hammer = TestBoosters.Create<HammerBoosterDefinition>("hammer", unlockLevel: 3, startingCount: 1, coinCost: 50);
            rocket = TestBoosters.Create<RocketBoosterDefinition>("rocket", unlockLevel: 5, startingCount: 1, coinCost: 70);
            boosters = new BoosterDefinition[] { freeze, hammer, rocket };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(freeze);
            Object.DestroyImmediate(hammer);
            Object.DestroyImmediate(rocket);
            Object.DestroyImmediate(economy);
        }

        [Test]
        public void NothingUnlocksBeforeTheFirstBoostersLevel()
        {
            Assert.IsEmpty(BoosterUnlockRules.Pending(boosters, 1, inventory));
        }

        [Test]
        public void ALevelUnlocksItsBoosterAndAnyTheSaveMissed()
        {
            CollectionAssert.AreEqual(new BoosterDefinition[] { freeze, hammer }, BoosterUnlockRules.Pending(boosters, 3, inventory),
                "A save that skipped level 2 still gets the freeze, in catalog order.");
        }

        [Test]
        public void AClaimedBoosterIsNotAnnouncedAgain()
        {
            inventory.Unlock(freeze);

            CollectionAssert.AreEqual(new BoosterDefinition[] { hammer }, BoosterUnlockRules.Pending(boosters, 4, inventory));
        }
    }
}
