using ColorBlockJam.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Level", fileName = "LevelInstaller")]
    public sealed class LevelInstaller : ScriptableInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<AutoPlayer>(Lifetime.Singleton);
            builder.Register<LevelBoard>(Lifetime.Singleton);
            builder.Register<SolvabilityWatcher>(Lifetime.Singleton);
            builder.Register<LevelResults>(Lifetime.Singleton);
            builder.Register<BlockPressRouter>(Lifetime.Singleton);

            builder.RegisterEntryPoint<BlockDragController>().AsSelf();
            builder.RegisterEntryPoint<LevelSession>().AsSelf();
            builder.RegisterEntryPoint<PauseRequester>().AsSelf();
            builder.RegisterEntryPoint<LevelIntros>();
            builder.RegisterEntryPoint<GameplayHudPresenter>();
        }
    }
}
