using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;

namespace ColorBlockJam.LevelEditor.Authoring
{
    public static class LevelRating
    {
        public const int MostEasyMoves = 8;
        public const int MostMediumMoves = 12;
        public const int MostHardMoves = 16;

        public static int MostMovesFor(LevelDifficulty difficulty)
        {
            return difficulty switch
            {
                LevelDifficulty.Easy => MostEasyMoves,
                LevelDifficulty.Medium => MostMediumMoves,
                LevelDifficulty.Hard => MostHardMoves,
                _ => int.MaxValue
            };
        }

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
