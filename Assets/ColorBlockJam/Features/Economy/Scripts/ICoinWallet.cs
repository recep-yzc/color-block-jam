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
    }
}
