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
        [SerializeField] private SplashConfig config;
        [SerializeField] private LoadingBarView loadingBar;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.RegisterComponent(loadingBar);
            builder.RegisterEntryPoint<SplashFlow>();
        }
    }
}
