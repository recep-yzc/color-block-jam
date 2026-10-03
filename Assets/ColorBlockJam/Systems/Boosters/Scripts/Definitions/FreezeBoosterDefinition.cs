using UnityEngine;

namespace ColorBlockJam.Boosters
{
    [CreateAssetMenu(menuName = "Color Block Jam/Boosters/Freeze", fileName = "Freeze")]
    public sealed class FreezeBoosterDefinition : InstantBoosterDefinition
    {
        [Tooltip("Seviye süresinin durduğu oyun süresi, saniye.")]
        [SerializeField, Min(1f)] private float seconds = 10f;

        protected override InstantBoosterEffect CreateInstantEffect(BoosterContext context)
        {
            return new FreezeEffect(context.Session.Timer, seconds);
        }
    }
}
