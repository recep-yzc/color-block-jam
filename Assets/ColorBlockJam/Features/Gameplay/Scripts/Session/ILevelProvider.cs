using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay
{
    public interface ILevelProvider
    {
        int LevelNumber { get; }

        /// <summary>True when the level comes from the level editor instead of the player's progress.</summary>
        bool IsEditorTest { get; }

        LevelData Load();
    }
}
