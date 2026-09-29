using Framework.Core.Installers;
using Framework.Core.Startup;
using UnityEngine;
using VContainer;

namespace Framework.Boot
{
    [CreateAssetMenu(menuName = "Framework/Installers/App Settings", fileName = "AppSettingsInstaller")]
    public sealed class AppSettingsInstaller : ScriptableInstaller
    {
        [SerializeField] private AppSettings settings;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(settings);
            builder.Register<ApplyAppSettingsTask>(Lifetime.Singleton).As<IStartupTask>();
        }
    }
}
