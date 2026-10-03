using ColorBlockJam.Core.Installers;
using ColorBlockJam.Gameplay;
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
            builder.Register<SeenObstacles>(Lifetime.Singleton).As<ISeenObstacles>();
            builder.Register<ObstacleIntros>(Lifetime.Singleton).As<ILevelIntro>();
        }
    }
}
