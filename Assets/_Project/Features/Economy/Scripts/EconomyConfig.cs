using UnityEngine;

namespace ColorBlockJam.Economy
{
    [CreateAssetMenu(menuName = "Color Block Jam/Economy/Economy Config", fileName = "EconomyConfig")]
    public sealed class EconomyConfig : ScriptableObject
    {
        [Tooltip("Coins of a new player.")]
        [SerializeField, Min(0)] private int startingCoins = 100;

        public int StartingCoins => startingCoins;
    }
}
