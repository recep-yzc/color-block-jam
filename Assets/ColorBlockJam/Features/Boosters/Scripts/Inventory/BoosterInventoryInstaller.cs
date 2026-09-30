using Framework.Core.Installers;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Boosters
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Booster Inventory", fileName = "BoosterInventoryInstaller")]
    public sealed class BoosterInventoryInstaller : ScriptableInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<BoosterInventory>(Lifetime.Singleton).As<IBoosterInventory>();
        }
    }
}
