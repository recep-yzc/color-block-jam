using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Framework.Core.Installers
{
    /// <summary>
    /// Registers the services of one module as an asset. Used for project-wide services
    /// that do not belong to a scene; add the asset to the application scope's installer list.
    /// </summary>
    public abstract class ScriptableInstaller : ScriptableObject, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
