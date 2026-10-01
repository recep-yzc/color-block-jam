using ColorBlockJam.UI.Views;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Economy
{
    public sealed class CoinHudView : UIView
    {
        [Tooltip("Oyuncunun coin miktarını gösteren yazı.")]
        [SerializeField] private TMP_Text amountLabel;

        public void SetCoins(int coins)
        {
            amountLabel.SetText("{0}", coins);
        }
    }
}
