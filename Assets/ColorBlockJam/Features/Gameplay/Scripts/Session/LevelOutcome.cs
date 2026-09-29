namespace ColorBlockJam.Gameplay
{
    public enum LevelFailReason
    {
        TimeUp,
        Stuck
    }

    /// <summary>How the level ended, for the result popups to show.</summary>
    public sealed class LevelOutcome
    {
        public LevelFailReason FailReason { get; private set; }
        public int Reward { get; private set; }

        public void Fail(LevelFailReason reason)
        {
            FailReason = reason;
        }

        public void Win(int reward)
        {
            Reward = reward;
        }
    }
}
