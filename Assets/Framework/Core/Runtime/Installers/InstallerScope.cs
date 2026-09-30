using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Framework.Core.Installers
{
    public sealed class InstallerScope : LifetimeScope
    {
        [Tooltip("Bu scope kurulurken servislerini kaydeden installer asset'leri.")]
        [SerializeField] private ScriptableInstaller[] assetInstallers = { };

        protected override void Configure(IContainerBuilder builder)
        {
            foreach (var installer in assetInstallers)
            {
                installer.Install(builder);
            }

            foreach (var installer in GetComponentsInChildren<MonoInstaller>(true))
            {
                installer.Install(builder);
            }
        }
    }
}
