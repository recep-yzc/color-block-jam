using System;
using System.Collections.Generic;
using ColorBlockJam.Gameplay;
using ColorBlockJam.Gameplay.Logic;
using VContainer.Unity;

namespace ColorBlockJam.Boosters
{
    public sealed class LevelBoosters : IBlockTargeting, IInitializable, IDisposable
    {
        private readonly LevelSession session;
        private readonly IBoosterInventory inventory;
        private readonly BlockPressRouter pressRouter;
        private readonly List<Booster> boosters = new();

        public LevelBoosters(BoosterCatalog catalog, BoosterContext context, LevelSession session, IBoosterInventory inventory,
            BlockPressRouter pressRouter)
        {
            this.session = session;
            this.inventory = inventory;
            this.pressRouter = pressRouter;
            foreach (var definition in catalog.Boosters)
            {
                boosters.Add(new Booster(definition, definition.CreateEffect(context)));
            }
        }

        public event Action AimingChanged;

        public IReadOnlyList<Booster> All => boosters;
        public Booster Aiming { get; private set; }
        public bool IsAiming => Aiming != null;

        public void Initialize()
        {
            pressRouter.Add(this);
            session.StateChanged += OnStateChanged;
        }

        public void Dispose()
        {
            pressRouter.Remove(this);
            session.StateChanged -= OnStateChanged;
        }

        public bool CanPress(Booster booster)
        {
            if (session.State != LevelState.Playing)
            {
                return false;
            }

            return booster == Aiming || (booster.Effect.IsReady && inventory.CanTake(booster.Definition));
        }

        public void Press(Booster booster)
        {
            if (!CanPress(booster))
            {
                return;
            }

            if (booster == Aiming)
            {
                PutBack();
                return;
            }

            PutBack();
            if (booster.Effect is AimedBoosterEffect)
            {
                SetAiming(booster);
            }
            else if (booster.Effect is InstantBoosterEffect instant && inventory.TryTake(booster.Definition))
            {
                instant.Apply();
            }
        }

        public void PutBack()
        {
            SetAiming(null);
        }

        public void Pick(BoardBlock block, GridPoint cell)
        {
            var booster = Aiming;
            PutBack();
            if (booster?.Effect is AimedBoosterEffect aimed && inventory.TryTake(booster.Definition))
            {
                aimed.Apply(block, cell);
            }
        }

        private void OnStateChanged()
        {
            if (session.State != LevelState.Playing)
            {
                PutBack();
            }
        }

        private void SetAiming(Booster booster)
        {
            if (Aiming == booster)
            {
                return;
            }

            Aiming = booster;
            AimingChanged?.Invoke();
        }
    }
}
