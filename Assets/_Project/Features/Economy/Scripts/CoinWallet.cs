using ColorBlockJam.Core.Persistence;

namespace ColorBlockJam.Economy
{
    public sealed class CoinWallet : ICoinWallet
    {
        private const string CoinsKey = "economy.coins";

        public CoinWallet(IKeyValueStorage storage, EconomyConfig config)
        {
            Coins = storage.GetInt(CoinsKey, config.StartingCoins);
        }

        public int Coins { get; }
    }
}
