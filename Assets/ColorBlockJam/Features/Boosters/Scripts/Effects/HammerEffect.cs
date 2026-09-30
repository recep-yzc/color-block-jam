using ColorBlockJam.Gameplay;
using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Boosters
{
    public sealed class HammerEffect : AimedBoosterEffect
    {
        private readonly LevelBoard board;

        public HammerEffect(LevelBoard board)
        {
            this.board = board;
        }

        public override void Apply(BoardBlock block, GridPoint cell)
        {
            board.Smash(block);
        }
    }
}
