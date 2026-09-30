using Framework.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Framework.Boot
{
    /// <summary>
    /// Runs the splash flow in the scene it is placed in.
    /// </summary>
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
