using ColorBlockJam.Economy;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using Framework.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The gameplay scene: the level session, the board and its input, the HUD and the result flow.
    /// The popups are added by the PopupInstaller next to this one.
    /// </summary>
    public sealed class GameplayInstaller : MonoInstaller
    {
        [Header("Data")]
        [SerializeField] private GameplayConfig config;
        [SerializeField] private BoardArt art;
        [SerializeField] private BlockPalette palette;
        [SerializeField] private LevelCatalog catalog;

        [Header("Scene")]
        [SerializeField] private Camera boardCamera;
        [SerializeField] private BoardView boardView;
        [Tooltip("Parent of the pooled block bursts.")]
        [SerializeField] private Transform effectsRoot;
        [SerializeField] private GameplayHudView hud;
        [SerializeField] private CoinHudView coinHud;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.RegisterInstance(art);
            builder.RegisterInstance(palette);
            builder.RegisterInstance(catalog);
            builder.RegisterComponent(boardCamera);
            builder.RegisterComponent(boardView);

            builder.Register<LevelProvider>(Lifetime.Singleton).As<ILevelProvider>();
            builder.Register<LevelFlow>(Lifetime.Singleton).As<ILevelFlow>();
            builder.Register<LevelOutcome>(Lifetime.Singleton);
            builder.Register<BoardSolver>(Lifetime.Singleton);
            builder.Register<AutoPlayer>(Lifetime.Singleton);
            builder.Register<BoardCamera>(Lifetime.Singleton);
            builder.Register(_ => new BlockBurstEffects(config.BurstPrefab, effectsRoot, config.BurstPrewarm), Lifetime.Singleton);

            builder.RegisterEntryPoint<BoardPointer>().AsSelf();
            builder.RegisterEntryPoint<BlockDragController>().AsSelf();
            builder.RegisterEntryPoint<LevelSession>().AsSelf();
            builder.RegisterEntryPoint<PauseRequester>().AsSelf();

            builder.RegisterComponent(hud);
            builder.RegisterEntryPoint<GameplayHudPresenter>();
            builder.RegisterCoinHud(coinHud);
        }
    }
}
