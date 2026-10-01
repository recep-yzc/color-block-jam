using System;
using System.Collections.Generic;
using ColorBlockJam.Economy;
using ColorBlockJam.Gameplay;
using ColorBlockJam.UI.Views;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterBarPresenter : ViewPresenter<BoosterBarView>, ITickable
    {
        private readonly LevelBoosters boosters;
        private readonly IBoosterInventory inventory;
        private readonly ICoinWallet wallet;
        private readonly LevelSession session;
        private readonly List<BoosterButtonView> buttons = new();
        private readonly List<Action> presses = new();
        private readonly List<bool> readiness = new();

        public BoosterBarPresenter(BoosterBarView view, LevelBoosters boosters, IBoosterInventory inventory, ICoinWallet wallet,
            LevelSession session)
            : base(view)
        {
            this.boosters = boosters;
            this.inventory = inventory;
            this.wallet = wallet;
            this.session = session;
        }

        protected override void OnInitialize()
        {
            foreach (var booster in boosters.All)
            {
                var button = View.AddButton(booster.Definition.Icon);
                Action press = () => boosters.Press(booster);
                button.Button.Clicked += press;
                if (!inventory.IsUnlocked(booster.Definition))
                {
                    button.HideImmediate();
                }

                buttons.Add(button);
                presses.Add(press);
                readiness.Add(booster.Effect.IsReady);
            }

            inventory.Changed += OnInventoryChanged;
            wallet.CoinsChanged += OnCoinsChanged;
            session.StateChanged += OnStateChanged;
            boosters.AimingChanged += Refresh;
            Refresh();
        }

        protected override void OnDispose()
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].Button.Clicked -= presses[i];
            }

            inventory.Changed -= OnInventoryChanged;
            wallet.CoinsChanged -= OnCoinsChanged;
            session.StateChanged -= OnStateChanged;
            boosters.AimingChanged -= Refresh;
        }

        public void Tick()
        {
            var all = boosters.All;
            for (var i = 0; i < all.Count; i++)
            {
                if (all[i].Effect.IsReady != readiness[i])
                {
                    Refresh();
                    return;
                }
            }
        }

        private void OnInventoryChanged(BoosterDefinition booster)
        {
            Refresh();
        }

        private void OnCoinsChanged(int coins)
        {
            if (boosters.IsAiming && !inventory.CanTake(boosters.Aiming.Definition))
            {
                boosters.PutBack();
            }

            Refresh();
        }

        private void OnStateChanged()
        {
            if (session.State != LevelState.Playing)
            {
                boosters.PutBack();
            }

            Refresh();
        }

        private void Refresh()
        {
            var all = boosters.All;
            for (var i = 0; i < all.Count; i++)
            {
                var booster = all[i];
                var button = buttons[i];
                readiness[i] = booster.Effect.IsReady;
                if (!inventory.IsUnlocked(booster.Definition))
                {
                    continue;
                }

                if (!button.IsVisible)
                {
                    button.ShowAsync().Forget();
                }

                button.SetStock(inventory.CountOf(booster.Definition), booster.Definition.CoinCost);
                button.Button.Interactable = boosters.CanPress(booster);
                button.SetAiming(booster == boosters.Aiming);
            }

            if (boosters.Aiming?.Definition is AimedBoosterDefinition aimed)
            {
                View.ShowAimHint(aimed.AimHint);
            }
            else
            {
                View.HideAimHint();
            }
        }
    }
}
