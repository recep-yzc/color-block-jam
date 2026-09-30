using System;

namespace ColorBlockJam.Boosters
{
    public interface IBoosterInventory
    {
        event Action<BoosterDefinition> Changed;

        bool IsUnlocked(BoosterDefinition booster);

        int CountOf(BoosterDefinition booster);

        void Unlock(BoosterDefinition booster);

        bool CanTake(BoosterDefinition booster);

        bool TryTake(BoosterDefinition booster);
    }
}
