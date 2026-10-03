using System;
using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using ColorBlockJam.LevelEditor.Authoring;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    [Serializable]
    public sealed class GeneratorPreset
    {
        [Tooltip("Bu ayarların ürettiği zorluk. Seviye bu rozeti taşır ve hamle sayısı bu zorluğun aralığında kalır.")]
        public LevelDifficulty difficulty;
        [Tooltip("Tahtanın genişliği, hücre.")]
        [Min(3)] public int width = 6;
        [Tooltip("Tahtanın yüksekliği, hücre.")]
        [Min(3)] public int height = 7;
        [Tooltip("Seviyede kullanılan renk sayısı. Her rengin en az bir kapısı ve bir bloğu olur.")]
        [Min(1)] public int colors = 5;
        [Tooltip("En az blok sayısı.")]
        [Min(1)] public int minBlocks = 7;
        [Tooltip("En çok blok sayısı.")]
        [Min(1)] public int maxBlocks = 9;
        [Tooltip("Bir kapının en uzun hali, hücre.")]
        [Min(1)] public int maxDoorLength = 3;
        [Tooltip("Çözümde en az kaç bloğun önce yoldan çekilmesi gerektiği.")]
        [Min(0)] public int minRepositions = 1;
        [Tooltip("Çözümde en çok kaç bloğun önce yoldan çekilebileceği.")]
        [Min(0)] public int maxRepositions = 3;
        [Tooltip("Kazanmak için en az hamle. En çok hamle, zorluğun rozet aralığından gelir.")]
        [Min(1)] public int minMoves = 9;
        [Tooltip("Çözücünün bir yerleşim için bakabileceği en çok durum. Büyük tahtalar daha fazlasını ister.")]
        [Min(100)] public int solveBudget = 10000;
        [Tooltip("Seviye süresinin sabit kısmı, saniye.")]
        [Min(0)] public int baseSeconds = 20;
        [Tooltip("Her blok için süreye eklenen saniye.")]
        [Min(0)] public int secondsPerBlock = 9;
        [Tooltip("Blokların ne kadarının ok bloğu olacağı. 0 = hiç, 1 = hepsi.")]
        [Range(0f, 1f)] public float arrowShare = 0.15f;
        [Tooltip("Buza gömülen blok sayısı.")]
        [Min(0)] public int iceBlocks;
        [Tooltip("Bir bloğun buzu en çok kaç bloğun çıkmasını bekler.")]
        [Min(1)] public int maxIce = 1;
        [Tooltip("Tek hücre, ikili, 2×2 kare ve üç hücrelik küçük L şekilleri kullanılır.")]
        public bool smallShapes = true;
        [Tooltip("Üç hücrelik çubuklar ve dört hücrelik L şekilleri kullanılır.")]
        public bool longShapes = true;
        [Tooltip("T şekilleri, uzun L'ler ve 2×3 blok kullanılır.")]
        public bool complexShapes;

        public GeneratorSettings ToSettings()
        {
            var pools = new List<IReadOnlyList<GridPoint[]>>();
            if (smallShapes)
            {
                pools.Add(BlockShapes.Small);
            }

            if (longShapes)
            {
                pools.Add(BlockShapes.Long);
            }

            if (complexShapes || pools.Count == 0)
            {
                pools.Add(BlockShapes.Complex);
            }

            return new GeneratorSettings
            {
                Difficulty = difficulty,
                Width = width,
                Height = height,
                Colors = colors,
                MinBlocks = minBlocks,
                MaxBlocks = Mathf.Max(minBlocks, maxBlocks),
                MaxDoorLength = maxDoorLength,
                MinRepositions = minRepositions,
                MaxRepositions = Mathf.Max(minRepositions, maxRepositions),
                MinMoves = minMoves,
                MaxMoves = LevelRating.MostMovesFor(difficulty),
                SolveBudget = solveBudget,
                BaseSeconds = baseSeconds,
                SecondsPerBlock = secondsPerBlock,
                ArrowShare = arrowShare,
                IceBlocks = iceBlocks,
                MaxIce = maxIce,
                ShapePools = pools.ToArray()
            };
        }

        public static GeneratorPreset From(GeneratorSettings settings)
        {
            var preset = new GeneratorPreset
            {
                difficulty = settings.Difficulty,
                width = settings.Width,
                height = settings.Height,
                colors = settings.Colors,
                minBlocks = settings.MinBlocks,
                maxBlocks = settings.MaxBlocks,
                maxDoorLength = settings.MaxDoorLength,
                minRepositions = settings.MinRepositions,
                maxRepositions = settings.MaxRepositions,
                minMoves = settings.MinMoves,
                solveBudget = settings.SolveBudget,
                baseSeconds = settings.BaseSeconds,
                secondsPerBlock = settings.SecondsPerBlock,
                arrowShare = (float)settings.ArrowShare,
                iceBlocks = settings.IceBlocks,
                maxIce = Math.Max(1, settings.MaxIce),
                smallShapes = false,
                longShapes = false,
                complexShapes = false
            };

            foreach (var pool in settings.ShapePools)
            {
                preset.smallShapes |= ReferenceEquals(pool, BlockShapes.Small);
                preset.longShapes |= ReferenceEquals(pool, BlockShapes.Long);
                preset.complexShapes |= ReferenceEquals(pool, BlockShapes.Complex);
            }

            return preset;
        }
    }
}
