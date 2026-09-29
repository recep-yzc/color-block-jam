using Framework.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Framework.Navigation
{
    /// <summary>
    /// Adds page and tab navigation to the scene.
    /// </summary>
    public sealed class NavigationInstaller : MonoInstaller
    {
        [SerializeField] private NavigationConfig config;
        [SerializeField] private PageNavigator pageNavigator;
        [SerializeField] private TabBar tabBar;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.RegisterComponent(pageNavigator);
            builder.RegisterComponent(tabBar);
            builder.RegisterEntryPoint<NavigationPresenter>();
        }
    }
}
