using System;

namespace ColorBlockJam.Economy
{
    /// <summary>
    /// The player's coin inventory. It is saved, so it keeps its value between levels and sessions.
    /// </summary>
    public interface ICoinWallet
    {
        event Action<int> CoinsChanged;

        int Coins { get; }

        void Add(int amount);

        /// <summary>Takes <paramref name="amount"/> coins if the player has that many.</summary>
        /// <returns>False, and nothing is taken, when there are not enough coins.</returns>
        bool TrySpend(int amount);
    }
}
