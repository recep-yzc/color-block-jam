using System;
using ColorBlockJam.Economy;
using Framework.Core.Persistence;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterInventory : IBoosterInventory
    {
        private readonly IKeyValueStorage storage;
        private readonly ICoinWallet wallet;

        public BoosterInventory(IKeyValueStorage storage, ICoinWallet wallet)
        {
            this.storage = storage;
            this.wallet = wallet;
        }

        public event Action<BoosterDefinition> Changed;

        public bool IsUnlocked(BoosterDefinition booster)
        {
            return storage.GetBool(UnlockedKey(booster), false);
        }

        public int CountOf(BoosterDefinition booster)
        {
            return storage.GetInt(CountKey(booster), 0);
        }

        public void Unlock(BoosterDefinition booster)
        {
            if (IsUnlocked(booster))
            {
                return;
            }

            storage.SetBool(UnlockedKey(booster), true);
            storage.SetInt(CountKey(booster), CountOf(booster) + booster.StartingCount);
            Changed?.Invoke(booster);
        }

        public bool CanTake(BoosterDefinition booster)
        {
            return IsUnlocked(booster) && (CountOf(booster) > 0 || wallet.Coins >= booster.CoinCost);
        }

        public bool TryTake(BoosterDefinition booster)
        {
            if (!IsUnlocked(booster))
            {
                return false;
            }

            var count = CountOf(booster);
            if (count == 0)
            {
                return wallet.TrySpend(booster.CoinCost);
            }

            storage.SetInt(CountKey(booster), count - 1);
            Changed?.Invoke(booster);
            return true;
        }

        private static string UnlockedKey(BoosterDefinition booster)
        {
            return "boosters." + booster.Id + ".unlocked";
        }

        private static string CountKey(BoosterDefinition booster)
        {
            return "boosters." + booster.Id + ".count";
        }
    }
}
