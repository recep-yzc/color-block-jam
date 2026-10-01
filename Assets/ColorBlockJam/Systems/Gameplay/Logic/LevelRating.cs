using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay.Logic
{
    public static class LevelRating
    {
        public const int MostEasyMoves = 8;
        public const int MostMediumMoves = 12;
        public const int MostHardMoves = 16;

        public static int MovesToWin(int blocks, SolveResult solution)
        {
            return blocks + solution.Repositions;
        }

        public static LevelDifficulty Rate(int movesToWin)
        {
            return movesToWin switch
            {
                <= MostEasyMoves => LevelDifficulty.Easy,
                <= MostMediumMoves => LevelDifficulty.Medium,
                <= MostHardMoves => LevelDifficulty.Hard,
                _ => LevelDifficulty.SuperHard
            };
        }
    }
}
