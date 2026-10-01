using UnityEngine;

namespace ColorBlockJam.Boosters
{
    [CreateAssetMenu(menuName = "Color Block Jam/Boosters/Vacuum", fileName = "Vacuum")]
    public sealed class VacuumBoosterDefinition : AimedBoosterDefinition
    {
        public override BoosterEffect CreateEffect(BoosterContext context)
        {
            return new VacuumEffect(context.Board);
        }
    }
}
