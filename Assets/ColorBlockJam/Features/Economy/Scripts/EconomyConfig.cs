using UnityEngine;

namespace ColorBlockJam.Economy
{
    [CreateAssetMenu(menuName = "Color Block Jam/Economy/Economy Config", fileName = "EconomyConfig")]
    public sealed class EconomyConfig : ScriptableObject
    {
        [Tooltip("Coins of a new player.")]
        [SerializeField, Min(0)] private int startingCoins = 100;

        [Tooltip("Coins given for completing a level.")]
        [SerializeField, Min(1)] private int levelCompleteReward = 10;

        public int StartingCoins => startingCoins;
        public int LevelCompleteReward => levelCompleteReward;
    }
}
