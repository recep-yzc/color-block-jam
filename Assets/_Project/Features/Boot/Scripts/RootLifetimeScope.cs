using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.SceneManagement;
using ColorBlockJam.Core.Startup;
using ColorBlockJam.Settings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Boot
{
    /// <summary>
    /// Project-wide composition root. VContainerSettings creates it before the first scene
    /// and every scene scope uses it as parent, so services registered here live for the whole session.
    /// </summary>
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private AppSettings appSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(appSettings);

            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<PlayerPrefsStorage>(Lifetime.Singleton).As<IKeyValueStorage>();

            builder.Register<SettingsService>(Lifetime.Singleton).As<ISettingsService>();
            builder.Register<HapticService>(Lifetime.Singleton).As<IHapticService>();

            builder.Register<ApplyAppSettingsTask>(Lifetime.Singleton).As<IStartupTask>();
        }
    }
}
