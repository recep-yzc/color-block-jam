using ColorBlockJam.Core.Persistence;

namespace ColorBlockJam.Progression
{
    public sealed class ProgressionService : IProgressionService
    {
        private const string CurrentLevelKey = "progression.currentLevel";
        private const int FirstLevel = 1;

        public ProgressionService(IKeyValueStorage storage)
        {
            CurrentLevel = storage.GetInt(CurrentLevelKey, FirstLevel);
        }

        public int CurrentLevel { get; }
    }
}
