using ColorBlockJam.Economy;

namespace ColorBlockJam.Gameplay
{
    /// <summary>Stops the level timer for a while, for coins. One freeze runs at a time.</summary>
    public sealed class FreezeBooster
    {
        private readonly LevelSession session;
        private readonly ICoinWallet wallet;
        private readonly GameplayConfig config;

        public FreezeBooster(LevelSession session, ICoinWallet wallet, GameplayConfig config)
        {
            this.session = session;
            this.wallet = wallet;
            this.config = config;
        }

        public int Cost => config.FreezeCost;

        public bool CanUse => session.State == LevelState.Playing && !session.Timer.IsFrozen && wallet.Coins >= Cost;

        public void Use()
        {
            if (CanUse && wallet.TrySpend(Cost))
            {
                session.Timer.Freeze(config.FreezeSeconds);
            }
        }
    }
}
