using ColorBlockJam.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Obstacles
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Level Obstacles", fileName = "LevelObstaclesInstaller")]
    public sealed class LevelObstaclesInstaller : ScriptableInstaller
    {
        [Tooltip("Seviyenin başında tanıtılacak engeller.")]
        [SerializeField] private ObstacleCatalog catalog;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(catalog);
            builder.Register<ObstacleIntroductions>(Lifetime.Singleton).As<IObstacleIntroductions>();
            builder.RegisterEntryPoint<ObstacleIntros>().AsSelf();
        }
    }
}
