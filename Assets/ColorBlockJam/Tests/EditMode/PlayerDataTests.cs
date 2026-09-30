using ColorBlockJam.Economy;
using ColorBlockJam.Progression;
using Framework.Core.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace ColorBlockJam.Tests
{
    public sealed class PlayerDataTests
    {
        [Test]
        public void NewPlayerGetsStartingCoins()
        {
            var config = ScriptableObject.CreateInstance<EconomyConfig>();

            var wallet = new CoinWallet(new InMemoryStorage(), config);

            Assert.AreEqual(config.StartingCoins, wallet.Coins);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SavedCoinsWinOverStartingCoins()
        {
            var config = ScriptableObject.CreateInstance<EconomyConfig>();
            var storage = new InMemoryStorage();
            storage.SetInt("economy.coins", 7);

            var wallet = new CoinWallet(storage, config);

            Assert.AreEqual(7, wallet.Coins);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SpendingNeedsEnoughCoinsAndIsSaved()
        {
            var config = ScriptableObject.CreateInstance<EconomyConfig>();
            var storage = new InMemoryStorage();
            storage.SetInt("economy.coins", 50);
            var wallet = new CoinWallet(storage, config);

            Assert.IsFalse(wallet.TrySpend(60));
            Assert.AreEqual(50, wallet.Coins, "A failed spend takes nothing.");
            Assert.IsTrue(wallet.TrySpend(40));
            Assert.AreEqual(10, new CoinWallet(storage, config).Coins, "What is left is saved.");
            Object.DestroyImmediate(config);
        }

        [Test]
        public void NewPlayerStartsAtLevelOne()
        {
            Assert.AreEqual(1, new ProgressionService(new InMemoryStorage()).CurrentLevel);
        }
    }
}
