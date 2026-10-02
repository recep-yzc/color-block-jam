using System;
using System.Threading;
using ColorBlockJam.Gameplay;
using ColorBlockJam.UI.Windows;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterUnlocks : IStartable, IDisposable
    {
        private readonly BoosterCatalog catalog;
        private readonly ILevelProvider levels;
        private readonly IBoosterInventory inventory;
        private readonly IWindows windows;
        private readonly CancellationTokenSource lifetime = new();

        public BoosterUnlocks(BoosterCatalog catalog, ILevelProvider levels, IBoosterInventory inventory, IWindows windows)
        {
            this.catalog = catalog;
            this.levels = levels;
            this.inventory = inventory;
            this.windows = windows;
        }

        public void Start()
        {
            if (!levels.IsEditorTest)
            {
                PresentAsync().Forget();
            }
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }

        private async UniTaskVoid PresentAsync()
        {
            var token = lifetime.Token;
            foreach (var booster in BoosterUnlockRules.Pending(catalog.Boosters, levels.LevelNumber, inventory))
            {
                var (isCanceled, _) = await windows.Get<BoosterUnlockPopupPresenter>().ShowAsync(booster, token).SuppressCancellationThrow();
                if (isCanceled)
                {
                    return;
                }

                inventory.Unlock(booster);
            }
        }
    }
}
