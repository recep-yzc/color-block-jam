using ColorBlockJam.Gameplay;
using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Boosters
{
    public sealed class RocketEffect : AimedBoosterEffect
    {
        private readonly LevelBoard board;

        public RocketEffect(LevelBoard board)
        {
            this.board = board;
        }

        public override void Apply(BoardBlock block, GridPoint cell)
        {
            board.Smash(BoardTargets.InRow(board.Board, cell.Y));
        }
    }
}
