using Framework.Core.Installers;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Economy
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Economy", fileName = "EconomyInstaller")]
    public sealed class EconomyInstaller : ScriptableInstaller
    {
        [SerializeField] private EconomyConfig config;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.Register<CoinWallet>(Lifetime.Singleton).As<ICoinWallet>();
        }
    }
}
