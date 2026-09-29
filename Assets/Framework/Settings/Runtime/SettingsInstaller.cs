using Framework.Core.Installers;
using UnityEngine;
using VContainer;

namespace Framework.Settings
{
    /// <summary>
    /// Project-wide player settings and haptics. Needs the core services for storage.
    /// </summary>
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
