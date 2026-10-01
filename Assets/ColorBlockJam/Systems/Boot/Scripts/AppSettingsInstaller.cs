using ColorBlockJam.Core.Installers;
using ColorBlockJam.Core.Startup;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Boot
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/App Settings", fileName = "AppSettingsInstaller")]
    public sealed class AppSettingsInstaller : ScriptableInstaller
    {
        [Tooltip("Kare hızı ve ekranın uyumaması gibi uygulama ayarları.")]
        [SerializeField] private AppSettings settings;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(settings);
            builder.Register<ApplyAppSettingsTask>(Lifetime.Singleton).As<IStartupTask>();
        }
    }
}
