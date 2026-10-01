using UnityEngine;

namespace ColorBlockJam.Economy
{
    [CreateAssetMenu(menuName = "Color Block Jam/Economy/Economy Config", fileName = "EconomyConfig")]
    public sealed class EconomyConfig : ScriptableObject
    {
        [Tooltip("Yeni bir oyuncunun başladığı coin miktarı.")]
        [SerializeField, Min(0)] private int startingCoins = 100;

        [Tooltip("Bir seviyeyi bitirince kazanılan coin.")]
        [SerializeField, Min(1)] private int levelCompleteReward = 10;

        public int StartingCoins => startingCoins;
        public int LevelCompleteReward => levelCompleteReward;
    }
}
