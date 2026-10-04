using ColorBlockJam.Core.Installers;
using ColorBlockJam.Core.Startup;
using ColorBlockJam.Level;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.Gameplay
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Board Warmup", fileName = "BoardWarmupInstaller")]
    public sealed class BoardWarmupInstaller : ScriptableInstaller
    {
        [Tooltip("Oyunun ayarları. Isıtma tahtası blok ve patlama prefab'larını buradan alır.")]
        [SerializeField] private GameplayConfig config;
        [Tooltip("Tahtanın ve blokların mesh ve materyalleri.")]
        [SerializeField] private BoardArt art;
        [Tooltip("Blok ve kapı renkleri.")]
        [SerializeField] private BlockPalette palette;
        [Tooltip("Oyun sahnesindeki ana ışığın gölge tipi. Aynı olmalı ki ısıtma, oyunun kullandığı gölge varyantını hazırlasın.")]
        [SerializeField] private LightShadows mainLightShadows = LightShadows.Hard;

        public override void Install(IContainerBuilder builder)
        {
            builder.Register(_ => new BoardWarmup(art, palette, config, mainLightShadows), Lifetime.Singleton).As<IStartupTask>();
        }
    }
}
