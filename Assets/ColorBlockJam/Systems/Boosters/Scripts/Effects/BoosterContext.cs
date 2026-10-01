using ColorBlockJam.Gameplay;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterContext
    {
        public BoosterContext(LevelSession session, LevelBoard board)
        {
            Session = session;
            Board = board;
        }

        public LevelSession Session { get; }
        public LevelBoard Board { get; }
    }
}
