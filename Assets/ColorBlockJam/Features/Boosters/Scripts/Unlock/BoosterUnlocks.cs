using System;
using System.Collections.Generic;
using System.Threading;
using ColorBlockJam.Gameplay;
using Cysharp.Threading.Tasks;
using Framework.UI.Popups;
using VContainer.Unity;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterUnlocks : IStartable, IDisposable
    {
        private readonly BoosterCatalog catalog;
        private readonly ILevelProvider levels;
        private readonly IBoosterInventory inventory;
        private readonly IPopupService popups;
        private readonly Queue<BoosterDefinition> pending = new();
        private readonly CancellationTokenSource lifetime = new();

        public BoosterUnlocks(BoosterCatalog catalog, ILevelProvider levels, IBoosterInventory inventory, IPopupService popups)
        {
            this.catalog = catalog;
            this.levels = levels;
            this.inventory = inventory;
            this.popups = popups;
        }

        public BoosterDefinition Current { get; private set; }

        public void Start()
        {
            if (levels.IsEditorTest)
            {
                return;
            }

            foreach (var booster in BoosterUnlockRules.Pending(catalog.Boosters, levels.LevelNumber, inventory))
            {
                pending.Enqueue(booster);
            }

            ShowNext();
        }

        public void Dispose()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }

        public void Claim()
        {
            var booster = Current;
            if (booster == null)
            {
                return;
            }

            Current = null;
            ClaimAsync(booster).Forget();
        }

        private async UniTaskVoid ClaimAsync(BoosterDefinition booster)
        {
            if (await popups.HideAsync<BoosterUnlockPopup>(lifetime.Token).SuppressCancellationThrow())
            {
                return;
            }

            inventory.Unlock(booster);
            ShowNext();
        }

        private void ShowNext()
        {
            if (pending.Count == 0)
            {
                return;
            }

            Current = pending.Dequeue();
            popups.ShowAsync<BoosterUnlockPopup>(lifetime.Token).Forget();
        }
    }
}
