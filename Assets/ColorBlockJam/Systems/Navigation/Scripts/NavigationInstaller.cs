using ColorBlockJam.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Navigation
{
    public sealed class NavigationInstaller : MonoInstaller
    {
        [Tooltip("Sayfa sırası, kaydırma ve sekme ayarları.")]
        [SerializeField] private NavigationConfig config;
        [Tooltip("Sayfaları sağa sola kaydıran gezgin.")]
        [SerializeField] private PageNavigator pageNavigator;
        [Tooltip("Alttaki sekme çubuğu.")]
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
