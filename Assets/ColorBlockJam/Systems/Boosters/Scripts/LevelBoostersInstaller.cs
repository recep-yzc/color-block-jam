using ColorBlockJam.Core.Installers;
using ColorBlockJam.Gameplay;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Boosters
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Level Boosters", fileName = "LevelBoostersInstaller")]
    public sealed class LevelBoostersInstaller : ScriptableInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<BoosterContext>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LevelBoosters>().AsSelf();
            builder.Register<BoosterUnlocks>(Lifetime.Singleton).As<ILevelIntro>();
            builder.RegisterEntryPoint<BoosterBarPresenter>();
        }
    }
}
