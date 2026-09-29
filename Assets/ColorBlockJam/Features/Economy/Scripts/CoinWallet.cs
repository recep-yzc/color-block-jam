using System;
using Framework.Core.Persistence;

namespace ColorBlockJam.Economy
{
    public sealed class CoinWallet : ICoinWallet
    {
        private const string CoinsKey = "economy.coins";

        private readonly IKeyValueStorage storage;

        public CoinWallet(IKeyValueStorage storage, EconomyConfig config)
        {
            this.storage = storage;
            Coins = storage.GetInt(CoinsKey, config.StartingCoins);
        }

        public event Action<int> CoinsChanged;

        public int Coins { get; private set; }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Only positive amounts can be added.");
            }

            Coins += amount;
            storage.SetInt(CoinsKey, Coins);
            CoinsChanged?.Invoke(Coins);
        }
    }
}
