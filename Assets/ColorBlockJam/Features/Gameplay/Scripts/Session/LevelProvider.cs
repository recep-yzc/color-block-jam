using ColorBlockJam.Level;
using ColorBlockJam.Progression;

namespace ColorBlockJam.Gameplay
{
    /// <summary>The player's current level from the catalog, or the level the level editor is testing.</summary>
    public sealed class LevelProvider : ILevelProvider
    {
        private readonly LevelCatalog catalog;
        private readonly IProgressionService progression;

        public LevelProvider(LevelCatalog catalog, IProgressionService progression)
        {
            this.catalog = catalog;
            this.progression = progression;
        }

        public int LevelNumber => progression.CurrentLevel;
        public bool IsEditorTest => EditorTestLevel.TryGet(out _);

        public LevelData Load()
        {
            return EditorTestLevel.TryGet(out var level) ? level : catalog.Load(progression.CurrentLevel);
        }
    }
}
