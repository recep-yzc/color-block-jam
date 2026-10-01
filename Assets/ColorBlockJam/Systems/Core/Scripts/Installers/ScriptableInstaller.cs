using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Core.Installers
{
    public abstract class ScriptableInstaller : ScriptableObject, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
