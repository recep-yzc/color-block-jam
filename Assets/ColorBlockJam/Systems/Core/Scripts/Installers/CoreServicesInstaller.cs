using ColorBlockJam.Core.Persistence;
using ColorBlockJam.Core.SceneManagement;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Core.Installers
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Core Services", fileName = "CoreServicesInstaller")]
    public sealed class CoreServicesInstaller : ScriptableInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<PlayerPrefsStorage>(Lifetime.Singleton).As<IKeyValueStorage>();
        }
    }
}
