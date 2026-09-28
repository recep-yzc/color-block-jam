using ColorBlockJam.Shared.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Home
{
    /// <summary>
    /// Composition root of the main scene.
    /// </summary>
    public sealed class HomeLifetimeScope : LifetimeScope
    {
        [SerializeField] private NavigationConfig navigationConfig;
        [SerializeField] private PageNavigator pageNavigator;
        [SerializeField] private TabBar tabBar;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(navigationConfig);
            builder.RegisterComponent(pageNavigator);
            builder.RegisterComponent(tabBar);

            builder.RegisterEntryPoint<NavigationPresenter>();
        }
    }
}
