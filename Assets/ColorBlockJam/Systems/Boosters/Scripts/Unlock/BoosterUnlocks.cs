using System.Threading;
using ColorBlockJam.Gameplay;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterUnlocks : ILevelIntro
    {
        private readonly BoosterCatalog catalog;
        private readonly ILevelProvider levels;
        private readonly IBoosterInventory inventory;
        private readonly IWindows windows;

        public BoosterUnlocks(BoosterCatalog catalog, ILevelProvider levels, IBoosterInventory inventory, IWindows windows)
        {
            this.catalog = catalog;
            this.levels = levels;
            this.inventory = inventory;
            this.windows = windows;
        }

        public async UniTask PresentAsync(CancellationToken cancellationToken)
        {
            foreach (var booster in BoosterUnlockRules.Pending(catalog.Boosters, levels.LevelNumber, inventory))
            {
                if (!await windows.Get<BoosterUnlockPopupPresenter>().ShowAsync(booster, cancellationToken))
                {
                    return;
                }

                inventory.Unlock(booster);
            }
        }
    }
}
