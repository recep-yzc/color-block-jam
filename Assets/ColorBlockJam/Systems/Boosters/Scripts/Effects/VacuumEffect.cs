using ColorBlockJam.Gameplay;
using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Boosters
{
    public sealed class VacuumEffect : AimedBoosterEffect
    {
        private readonly LevelBoard board;

        public VacuumEffect(LevelBoard board)
        {
            this.board = board;
        }

        public override void Apply(BoardBlock block, GridPoint cell)
        {
            foreach (var target in BoardTargets.OfColor(board.Board, block.Color))
            {
                board.Smash(target);
            }
        }
    }
}
