using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Boosters
{
    public abstract class AimedBoosterEffect : BoosterEffect
    {
        public abstract void Apply(BoardBlock block, GridPoint cell);
    }
}
