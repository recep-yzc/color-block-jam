using ColorBlockJam.Core.Installers;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Economy
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Economy", fileName = "EconomyInstaller")]
    public sealed class EconomyInstaller : ScriptableInstaller
    {
        [Tooltip("Başlangıç coin'ini ve seviye ödülünü tutan ekonomi ayarları.")]
        [SerializeField] private EconomyConfig config;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.Register<CoinWallet>(Lifetime.Singleton).As<ICoinWallet>();
        }
    }
}
