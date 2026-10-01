using System;
using ColorBlockJam.Core.Persistence;

namespace ColorBlockJam.Progression
{
    public sealed class ProgressionService : IProgressionService
    {
        private const string CurrentLevelKey = "progression.currentLevel";
        private const int FirstLevel = 1;

        private readonly IKeyValueStorage storage;

        public ProgressionService(IKeyValueStorage storage)
        {
            this.storage = storage;
            CurrentLevel = storage.GetInt(CurrentLevelKey, FirstLevel);
        }

        public int CurrentLevel { get; private set; }

        public void CompleteCurrentLevel()
        {
            SetCurrentLevel(CurrentLevel + 1);
        }

        public void SetCurrentLevel(int level)
        {
            CurrentLevel = Math.Max(FirstLevel, level);
            storage.SetInt(CurrentLevelKey, CurrentLevel);
        }
    }
}
