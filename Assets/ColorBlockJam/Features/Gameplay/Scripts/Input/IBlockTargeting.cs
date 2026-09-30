using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Something waiting for the player to pick a block, such as the hammer. While it aims, a press on a block goes to
    /// it instead of starting a drag.
    /// </summary>
    public interface IBlockTargeting
    {
        bool IsAiming { get; }

        void Pick(BoardBlock block);
    }
}
