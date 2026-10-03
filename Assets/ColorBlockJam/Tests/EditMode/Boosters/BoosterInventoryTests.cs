using ColorBlockJam.Boosters;
using ColorBlockJam.Economy;
using NUnit.Framework;
using UnityEngine;

namespace ColorBlockJam.Tests
{
    public sealed class BoosterInventoryTests
    {
        private InMemoryStorage storage;
        private EconomyConfig economy;
        private CoinWallet wallet;
        private BoosterInventory inventory;
        private HammerBoosterDefinition hammer;

        [SetUp]
        public void SetUp()
        {
            storage = new InMemoryStorage();
            economy = ScriptableObject.CreateInstance<EconomyConfig>();
            storage.SetInt("economy.coins", 100);
            wallet = new CoinWallet(storage, economy);
            inventory = new BoosterInventory(storage, wallet);
            hammer = TestBoosters.Create<HammerBoosterDefinition>("hammer", unlockLevel: 3, startingCount: 2, coinCost: 50);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(hammer);
            Object.DestroyImmediate(economy);
        }

        [Test]
        public void ABoosterStartsLockedAndEmpty()
        {
            Assert.IsFalse(inventory.IsUnlocked(hammer));
            Assert.AreEqual(0, inventory.CountOf(hammer));
            Assert.IsFalse(inventory.CanTake(hammer));
        }

        [Test]
        public void UnlockingGivesTheStartingCountOnlyOnce()
        {
            inventory.Unlock(hammer);
            inventory.Unlock(hammer);

            Assert.IsTrue(inventory.IsUnlocked(hammer));
            Assert.AreEqual(2, inventory.CountOf(hammer));
        }

        [Test]
        public void AUseTakesAnOwnedBoosterBeforeAnyCoins()
        {
            inventory.Unlock(hammer);

            Assert.IsTrue(inventory.TryTake(hammer));

            Assert.AreEqual(1, inventory.CountOf(hammer));
            Assert.AreEqual(100, wallet.Coins, "Coins are only spent once none are left.");
        }

        [Test]
        public void WithNoneLeftAUseIsBoughtWithCoins()
        {
            inventory.Unlock(hammer);
            inventory.TryTake(hammer);
            inventory.TryTake(hammer);

            Assert.IsTrue(inventory.TryTake(hammer));

            Assert.AreEqual(0, inventory.CountOf(hammer));
            Assert.AreEqual(50, wallet.Coins);
        }

        [Test]
        public void WithoutBoostersOrCoinsNothingIsTaken()
        {
            wallet.TrySpend(100);
            inventory.Unlock(hammer);
            inventory.TryTake(hammer);
            inventory.TryTake(hammer);

            Assert.IsFalse(inventory.CanTake(hammer));
            Assert.IsFalse(inventory.TryTake(hammer));
            Assert.AreEqual(0, wallet.Coins);
        }

        [Test]
        public void ALockedBoosterCannotBeBoughtEvenWithCoins()
        {
            Assert.IsFalse(inventory.CanTake(hammer));
            Assert.IsFalse(inventory.TryTake(hammer));
            Assert.AreEqual(100, wallet.Coins);
        }

        [Test]
        public void BoostersAreSaved()
        {
            inventory.Unlock(hammer);
            inventory.TryTake(hammer);

            var reloaded = new BoosterInventory(storage, new CoinWallet(storage, economy));

            Assert.IsTrue(reloaded.IsUnlocked(hammer));
            Assert.AreEqual(1, reloaded.CountOf(hammer));
        }

        [Test]
        public void EveryChangeOfCountOrLockIsAnnounced()
        {
            var changes = 0;
            inventory.Changed += () => changes++;

            inventory.Unlock(hammer);
            inventory.TryTake(hammer);

            Assert.AreEqual(2, changes);
        }
    }
}
