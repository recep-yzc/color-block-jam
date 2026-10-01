using ColorBlockJam.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Boosters
{
    public sealed class BoostersInstaller : MonoInstaller
    {
        [Tooltip("Oyundaki booster'lar, bardaki sırasıyla.")]
        [SerializeField] private BoosterCatalog catalog;
        [Tooltip("Tahtanın altındaki booster çubuğu.")]
        [SerializeField] private BoosterBarView bar;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(catalog);
            builder.Register<BoosterContext>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LevelBoosters>().AsSelf();
            builder.RegisterEntryPoint<BoosterUnlocks>().AsSelf();

            builder.RegisterComponent(bar);
            builder.RegisterEntryPoint<BoosterBarPresenter>();
        }
    }
}
