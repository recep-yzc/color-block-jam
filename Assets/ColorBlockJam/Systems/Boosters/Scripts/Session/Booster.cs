namespace ColorBlockJam.Boosters
{
    public sealed class Booster
    {
        public Booster(BoosterDefinition definition, BoosterEffect effect)
        {
            Definition = definition;
            Effect = effect;
        }

        public BoosterDefinition Definition { get; }
        public BoosterEffect Effect { get; }
    }
}
