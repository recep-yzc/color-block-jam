using ColorBlockJam.UI.Buttons;
using ColorBlockJam.UI.Windows;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class OutOfTimePopup : WindowView
    {
        [Tooltip("Eklenecek süreyi saatin üstünde gösteren yazı.")]
        [SerializeField] private TMP_Text secondsLabel;
        [Tooltip("Ek sürenin ne işe yaradığını anlatan yazı.")]
        [SerializeField] private TMP_Text messageLabel;
        [Tooltip("Anlatım yazısının kalıbı. {0} eklenecek saniyeyle değişir.")]
        [SerializeField] private string messageFormat = "Get {0} seconds to keep playing!";
        [Tooltip("Ek sürenin coin bedelini gösteren yazı.")]
        [SerializeField] private TMP_Text priceLabel;
        [Tooltip("Coin yetiyorken bedelin rengi.")]
        [SerializeField] private Color affordableColor = Color.white;
        [Tooltip("Coin yetmiyorken bedelin rengi.")]
        [SerializeField] private Color unaffordableColor = new(1f, 0.16f, 0.16f);
        [Tooltip("Coin harcayıp ek süreyle oyuna devam eden buton. Coin yetmiyorsa basılmaz.")]
        [SerializeField] private ActionButton continueButton;

        public ActionButton ContinueButton => continueButton;

        public void SetOffer(int seconds, int price, bool canAfford)
        {
            secondsLabel.SetText("+{0}", seconds);
            messageLabel.SetText(messageFormat, seconds);
            priceLabel.SetText("{0}", price);
            priceLabel.color = canAfford ? affordableColor : unaffordableColor;
            continueButton.Interactable = canAfford;
        }
    }
}
