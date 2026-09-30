using System;
using ColorBlockJam.Economy;
using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Gameplay
{
    public sealed class HammerBooster : IBlockTargeting
    {
        private readonly LevelBoard levelBoard;
        private readonly ICoinWallet wallet;
        private readonly GameplayConfig config;

        public HammerBooster(LevelBoard levelBoard, ICoinWallet wallet, GameplayConfig config)
        {
            this.levelBoard = levelBoard;
            this.wallet = wallet;
            this.config = config;
        }

        public event Action AimingChanged;

        public bool IsAiming { get; private set; }
        public int Cost => config.HammerCost;
        public bool CanAfford => wallet.Coins >= Cost;

        public void ToggleAim()
        {
            SetAiming(!IsAiming && CanAfford);
        }

        public void PutBack()
        {
            SetAiming(false);
        }

        public void Pick(BoardBlock block, GridPoint cell)
        {
            SetAiming(false);
            if (wallet.TrySpend(Cost))
            {
                levelBoard.Smash(block);
            }
        }

        private void SetAiming(bool isAiming)
        {
            if (IsAiming == isAiming)
            {
                return;
            }

            IsAiming = isAiming;
            AimingChanged?.Invoke();
        }
    }
}
