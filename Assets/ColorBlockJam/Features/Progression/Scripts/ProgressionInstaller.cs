using Framework.Core.Installers;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Progression
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Progression", fileName = "ProgressionInstaller")]
    public sealed class ProgressionInstaller : ScriptableInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ProgressionService>(Lifetime.Singleton).As<IProgressionService>();
        }
    }
}
