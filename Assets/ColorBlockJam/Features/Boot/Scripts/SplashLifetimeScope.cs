using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Boot
{
    public sealed class SplashLifetimeScope : LifetimeScope
    {
        [SerializeField] private SplashConfig config;
        [SerializeField] private LoadingBarView loadingBar;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.RegisterComponent(loadingBar);

            builder.RegisterEntryPoint<SplashFlow>();
        }
    }
}
