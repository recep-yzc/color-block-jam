using ColorBlockJam.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class LevelScopeInstaller : MonoInstaller
    {
        [Tooltip("Her seviye için kurulan, seviye bitince ya da yeniden başlayınca kapatılan servisleri kaydeden installer'lar.")]
        [SerializeField] private ScriptableInstaller[] levelInstallers = { };

        public override void Install(IContainerBuilder builder)
        {
            var installers = levelInstallers;
            builder.RegisterEntryPoint<LevelRunner>().WithParameter<IInstaller>(new ActionInstaller(level =>
            {
                foreach (var installer in installers)
                {
                    installer.Install(level);
                }
            })).AsSelf();
        }
    }
}
