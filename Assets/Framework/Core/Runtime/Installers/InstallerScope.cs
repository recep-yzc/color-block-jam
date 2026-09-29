using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Framework.Core.Installers
{
    /// <summary>
    /// A lifetime scope composed from installers instead of code, so modules are added and removed
    /// in the Inspector. Used both for the project root (with <see cref="ScriptableInstaller"/> assets)
    /// and for scenes (with <see cref="MonoInstaller"/> components on this GameObject or under it).
    /// </summary>
    public sealed class InstallerScope : LifetimeScope
    {
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
