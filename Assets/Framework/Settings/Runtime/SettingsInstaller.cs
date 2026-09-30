using Framework.Core.Installers;
using UnityEngine;
using VContainer;

namespace Framework.Settings
{
    [CreateAssetMenu(menuName = "Framework/Installers/Settings", fileName = "SettingsInstaller")]
    public sealed class SettingsInstaller : ScriptableInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<SettingsService>(Lifetime.Singleton).As<ISettingsService>();
            builder.Register<HapticService>(Lifetime.Singleton).As<IHapticService>();
        }
    }
}
