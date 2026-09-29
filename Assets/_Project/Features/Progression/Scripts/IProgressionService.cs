namespace ColorBlockJam.Progression
{
    /// <summary>
    /// Which level the player plays next. Saved between sessions.
    /// </summary>
    public interface IProgressionService
    {
        /// <summary>1-based number of the level the player plays next.</summary>
        int CurrentLevel { get; }
    }
}
