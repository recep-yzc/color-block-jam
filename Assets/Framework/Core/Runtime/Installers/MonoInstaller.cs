using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Framework.Core.Installers
{
    /// <summary>
    /// Registers the services and views of one module in a scene.
    /// Put it on the same GameObject as an <see cref="InstallerScope"/>, or under it.
    /// </summary>
    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
