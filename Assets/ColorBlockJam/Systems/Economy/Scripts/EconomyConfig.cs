using System;
using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Economy
{
    [CreateAssetMenu(menuName = "Color Block Jam/Economy/Economy Config", fileName = "EconomyConfig")]
    public sealed class EconomyConfig : ScriptableObject
    {
        [Tooltip("Yeni bir oyuncunun başladığı coin miktarı.")]
        [SerializeField, Min(0)] private int startingCoins = 100;

        [Header("Level Rewards")]
        [Tooltip("Kolay bir seviyeyi bitirince kazanılan coin.")]
        [SerializeField, Min(1)] private int easyReward = 10;
        [Tooltip("Orta bir seviyeyi bitirince kazanılan coin.")]
        [SerializeField, Min(1)] private int mediumReward = 20;
        [Tooltip("Zor bir seviyeyi bitirince kazanılan coin.")]
        [SerializeField, Min(1)] private int hardReward = 30;
        [Tooltip("Süper zor bir seviyeyi bitirince kazanılan coin.")]
        [SerializeField, Min(1)] private int superHardReward = 50;

        public int StartingCoins => startingCoins;

        public int RewardFor(LevelDifficulty difficulty)
        {
            return difficulty switch
            {
                LevelDifficulty.Easy => easyReward,
                LevelDifficulty.Medium => mediumReward,
                LevelDifficulty.Hard => hardReward,
                LevelDifficulty.SuperHard => superHardReward,
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
            };
        }
    }
}
