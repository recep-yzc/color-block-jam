using ColorBlockJam.Economy;
using Framework.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Home
{
    public sealed class HomeInstaller : MonoInstaller
    {
        [Tooltip("Ana ekranın üst çubuğu.")]
        [SerializeField] private HomeHudView hud;
        [Tooltip("Coin miktarını gösteren sayaç.")]
        [SerializeField] private CoinHudView coinHud;
        [Tooltip("Seviye yolu ve oyna butonu.")]
        [SerializeField] private HomeLevelView level;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(hud);
            builder.RegisterEntryPoint<HomeHudPresenter>();

            builder.RegisterCoinHud(coinHud);

            builder.RegisterComponent(level);
            builder.RegisterEntryPoint<HomeLevelPresenter>();
        }
    }
}
