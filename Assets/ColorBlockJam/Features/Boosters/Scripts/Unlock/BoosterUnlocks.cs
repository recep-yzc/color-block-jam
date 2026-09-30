using ColorBlockJam.Gameplay;
using VContainer.Unity;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterUnlocks : IStartable
    {
        private readonly BoosterCatalog catalog;
        private readonly ILevelProvider levels;
        private readonly IBoosterInventory inventory;

        public BoosterUnlocks(BoosterCatalog catalog, ILevelProvider levels, IBoosterInventory inventory)
        {
            this.catalog = catalog;
            this.levels = levels;
            this.inventory = inventory;
        }

        public void Start()
        {
            if (levels.IsEditorTest)
            {
                return;
            }

            foreach (var booster in BoosterUnlockRules.Pending(catalog.Boosters, levels.LevelNumber, inventory))
            {
                inventory.Unlock(booster);
            }
        }
    }
}
