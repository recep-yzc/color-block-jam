using ColorBlockJam.Economy;
using NUnit.Framework;
using UnityEngine;

namespace ColorBlockJam.Tests
{
    public sealed class CoinWalletTests
    {
        private EconomyConfig config;
        private InMemoryStorage storage;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<EconomyConfig>();
            storage = new InMemoryStorage();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void NewPlayerGetsStartingCoins()
        {
            var wallet = new CoinWallet(storage, config);

            Assert.AreEqual(config.StartingCoins, wallet.Coins);
        }

        [Test]
        public void SavedCoinsWinOverStartingCoins()
        {
            storage.SetInt(CoinWallet.CoinsKey, 7);

            var wallet = new CoinWallet(storage, config);

            Assert.AreEqual(7, wallet.Coins);
        }

        [Test]
        public void SpendingNeedsEnoughCoinsAndIsSaved()
        {
            storage.SetInt(CoinWallet.CoinsKey, 50);
            var wallet = new CoinWallet(storage, config);

            Assert.IsFalse(wallet.TrySpend(60));
            Assert.AreEqual(50, wallet.Coins, "A failed spend takes nothing.");
            Assert.IsTrue(wallet.TrySpend(40));
            Assert.AreEqual(10, new CoinWallet(storage, config).Coins, "What is left is saved.");
        }
    }
}
