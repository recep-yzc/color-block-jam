using System;
using ColorBlockJam.Economy;
using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Breaks one block the player picks, for coins. Taking the hammer up only aims it: the next press on a block
    /// breaks that block instead of grabbing it, and only then are the coins paid. Taking it up again puts it back.
    /// </summary>
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

        /// <summary>Raised when the hammer is taken up or put back.</summary>
        public event Action AimingChanged;

        public bool IsAiming { get; private set; }
        public int Cost => config.HammerCost;
        public bool CanAfford => wallet.Coins >= Cost;

        /// <summary>Takes the hammer up when the player can pay for it, or puts it back.</summary>
        public void ToggleAim()
        {
            SetAiming(!IsAiming && CanAfford);
        }

        public void PutBack()
        {
            SetAiming(false);
        }

        public void Pick(BoardBlock block)
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
