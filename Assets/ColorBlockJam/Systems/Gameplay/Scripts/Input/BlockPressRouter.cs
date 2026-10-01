using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;

namespace ColorBlockJam.Gameplay
{
    public sealed class BlockPressRouter
    {
        private readonly List<IBlockTargeting> targets = new();

        public void Add(IBlockTargeting target)
        {
            targets.Add(target);
        }

        public void Remove(IBlockTargeting target)
        {
            targets.Remove(target);
        }

        public bool TryPick(BoardBlock block, GridPoint cell)
        {
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i].IsAiming)
                {
                    targets[i].Pick(block, cell);
                    return true;
                }
            }

            return false;
        }
    }
}
