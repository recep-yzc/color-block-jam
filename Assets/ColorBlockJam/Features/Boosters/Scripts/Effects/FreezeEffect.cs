using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Boosters
{
    public sealed class FreezeEffect : InstantBoosterEffect
    {
        private readonly LevelTimer timer;
        private readonly float seconds;

        public FreezeEffect(LevelTimer timer, float seconds)
        {
            this.timer = timer;
            this.seconds = seconds;
        }

        public override bool IsReady => !timer.IsFrozen;

        public override void Apply()
        {
            timer.Freeze(seconds);
        }
    }
}
