namespace ColorBlockJam.Boosters
{
    public abstract class InstantBoosterDefinition : BoosterDefinition
    {
        public sealed override BoosterEffect CreateEffect(BoosterContext context)
        {
            return CreateInstantEffect(context);
        }

        protected abstract InstantBoosterEffect CreateInstantEffect(BoosterContext context);
    }
}
