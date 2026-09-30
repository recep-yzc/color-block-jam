using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Gameplay
{
    public interface IBlockTargeting
    {
        bool IsAiming { get; }

        void Pick(BoardBlock block);
    }
}
