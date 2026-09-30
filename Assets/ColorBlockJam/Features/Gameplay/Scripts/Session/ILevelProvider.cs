using ColorBlockJam.Level;

namespace ColorBlockJam.Gameplay
{
    public interface ILevelProvider
    {
        int LevelNumber { get; }

        bool IsEditorTest { get; }

        LevelData Load();
    }
}
