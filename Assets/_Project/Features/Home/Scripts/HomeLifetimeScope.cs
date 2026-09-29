using ColorBlockJam.Settings;
using ColorBlockJam.Shared.Navigation;
using ColorBlockJam.Shared.UI.Popups;
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
        [SerializeField] private HomeHudView homeHud;
        [SerializeField] private HomeLevelView homeLevel;
        [SerializeField] private PopupLayer popupLayer;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(navigationConfig);
            builder.RegisterComponent(pageNavigator);
            builder.RegisterComponent(tabBar);
            builder.RegisterEntryPoint<NavigationPresenter>();

            builder.RegisterComponent(homeHud);
            builder.RegisterEntryPoint<HomeHudPresenter>();

            builder.RegisterComponent(homeLevel);
            builder.RegisterEntryPoint<HomeLevelPresenter>();

            builder.RegisterPopupLayer(popupLayer);
            builder.RegisterEntryPoint<SettingsPopupPresenter>();
        }
    }
}
