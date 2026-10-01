namespace ColorBlockJam.Progression
{
    public interface IProgressionService
    {
        int CurrentLevel { get; }

        void CompleteCurrentLevel();

        void SetCurrentLevel(int level);
    }
}
