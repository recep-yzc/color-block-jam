using ColorBlockJam.Core.Installers;
using ColorBlockJam.Economy;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class GameplayInstaller : MonoInstaller
    {
        [Header("Data")]
        [Tooltip("Sürükleme, animasyon, çözücü ve kamera ayarları.")]
        [SerializeField] private GameplayConfig config;
        [Tooltip("Tahtanın ve blokların kurulduğu mesh ve materyaller.")]
        [SerializeField] private BoardArt art;
        [Tooltip("Blok ve kapı renkleri. Seviyeler rengi bu listedeki sırasıyla tutar.")]
        [SerializeField] private BlockPalette palette;
        [Tooltip("Oynanış sırasıyla seviye dosyaları.")]
        [SerializeField] private LevelCatalog catalog;

        [Header("Scene")]
        [Tooltip("Tahtayı gösteren kamera. Tahtayı ekrana sığdıracak şekilde konumlanır.")]
        [SerializeField] private Camera boardCamera;
        [Tooltip("Tahtanın, kapıların ve blokların altında kurulduğu obje.")]
        [SerializeField] private BoardView boardView;
        [Tooltip("Havuzdan gelen blok patlamalarının altında durduğu obje.")]
        [SerializeField] private Transform effectsRoot;
        [Tooltip("Seviye, zorluk, süre, yeniden başlatma, duraklatma ve otomatik oynatmanın olduğu üst çubuk.")]
        [SerializeField] private GameplayHudView hud;
        [Tooltip("Coin miktarını gösteren sayaç.")]
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
            builder.Register<BoardSolver>(Lifetime.Singleton);
            builder.Register<AutoPlayer>(Lifetime.Singleton);
            builder.Register<LevelBoard>(Lifetime.Singleton);
            builder.Register<SolvabilityWatcher>(Lifetime.Singleton);
            builder.Register<LevelResults>(Lifetime.Singleton);
            builder.Register<BoardCamera>(Lifetime.Singleton);
            builder.Register(_ => new BlockBurstEffects(config.BurstPrefab, effectsRoot, config.BurstPrewarm), Lifetime.Singleton);
            builder.Register<BlockPressRouter>(Lifetime.Singleton);

            builder.RegisterEntryPoint<BoardPointer>().AsSelf();
            builder.RegisterEntryPoint<BlockDragController>().AsSelf();
            builder.RegisterEntryPoint<LevelSession>().AsSelf();
            builder.RegisterEntryPoint<PauseRequester>().AsSelf();
            builder.RegisterEntryPoint<LevelShortcuts>();

            builder.RegisterComponent(hud);
            builder.RegisterEntryPoint<GameplayHudPresenter>();
            builder.RegisterCoinHud(coinHud);
        }
    }
}
