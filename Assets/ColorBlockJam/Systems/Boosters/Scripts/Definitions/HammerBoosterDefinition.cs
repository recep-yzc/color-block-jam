using UnityEngine;

namespace ColorBlockJam.Boosters
{
    [CreateAssetMenu(menuName = "Color Block Jam/Boosters/Hammer", fileName = "Hammer")]
    public sealed class HammerBoosterDefinition : AimedBoosterDefinition
    {
        protected override AimedBoosterEffect CreateAimedEffect(BoosterContext context)
        {
            return new HammerEffect(context.Board);
        }
    }
}
