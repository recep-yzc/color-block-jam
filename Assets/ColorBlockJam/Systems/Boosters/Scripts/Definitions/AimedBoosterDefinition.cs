using UnityEngine;

namespace ColorBlockJam.Boosters
{
    public abstract class AimedBoosterDefinition : BoosterDefinition
    {
        [Tooltip("Booster kalkıkken oyuncuya ne yapacağını söyleyen ipucu.")]
        [SerializeField] private string aimHint = "Tap a block";

        public string AimHint => aimHint;

        public sealed override BoosterEffect CreateEffect(BoosterContext context)
        {
            return CreateAimedEffect(context);
        }

        protected abstract AimedBoosterEffect CreateAimedEffect(BoosterContext context);
    }
}
