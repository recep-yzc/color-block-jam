using System.Collections.Generic;

namespace ColorBlockJam.Boosters
{
    public static class BoosterUnlockRules
    {
        public static List<BoosterDefinition> Pending(IReadOnlyList<BoosterDefinition> boosters, int levelNumber,
            IBoosterInventory inventory)
        {
            var pending = new List<BoosterDefinition>();
            foreach (var booster in boosters)
            {
                if (booster.UnlockLevel <= levelNumber && !inventory.IsUnlocked(booster))
                {
                    pending.Add(booster);
                }
            }

            return pending;
        }
    }
}
