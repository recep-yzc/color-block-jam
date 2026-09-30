using UnityEngine;

namespace ColorBlockJam.Boosters
{
    [CreateAssetMenu(menuName = "Color Block Jam/Boosters/Rocket", fileName = "Rocket")]
    public sealed class RocketBoosterDefinition : AimedBoosterDefinition
    {
        public override BoosterEffect CreateEffect(BoosterContext context)
        {
            return new RocketEffect(context.Board);
        }
    }
}
