using Framework.UI.Views;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Economy
{
    /// <summary>
    /// Coin counter for any screen. Drop the CoinHud prefab in a scene and register it with
    /// <see cref="CoinHudContainerBuilderExtensions.RegisterCoinHud"/>.
    /// </summary>
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
