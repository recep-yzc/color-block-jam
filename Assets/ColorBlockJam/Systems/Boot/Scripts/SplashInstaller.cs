using ColorBlockJam.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Boot
{
    public sealed class SplashInstaller : MonoInstaller
    {
        [Tooltip("Açılış ekranı ayarları.")]
        [SerializeField] private SplashConfig config;
        [Tooltip("Açılış ekranındaki yükleme çubuğu.")]
        [SerializeField] private LoadingBarView loadingBar;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.RegisterComponent(loadingBar);
            builder.RegisterEntryPoint<SplashFlow>();
        }
    }
}
