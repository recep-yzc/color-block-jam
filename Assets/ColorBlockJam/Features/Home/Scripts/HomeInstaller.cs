using Framework.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// The home page content: top bar and level button.
    /// </summary>
    public sealed class HomeInstaller : MonoInstaller
    {
        [SerializeField] private HomeHudView hud;
        [SerializeField] private HomeLevelView level;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(hud);
            builder.RegisterEntryPoint<HomeHudPresenter>();

            builder.RegisterComponent(level);
            builder.RegisterEntryPoint<HomeLevelPresenter>();
        }
    }
}
