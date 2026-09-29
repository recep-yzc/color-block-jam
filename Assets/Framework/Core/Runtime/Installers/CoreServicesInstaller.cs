using Framework.Core.Persistence;
using Framework.Core.SceneManagement;
using UnityEngine;
using VContainer;

namespace Framework.Core.Installers
{
    [CreateAssetMenu(menuName = "Framework/Installers/Core Services", fileName = "CoreServicesInstaller")]
    public sealed class CoreServicesInstaller : ScriptableInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<PlayerPrefsStorage>(Lifetime.Singleton).As<IKeyValueStorage>();
        }
    }
}
