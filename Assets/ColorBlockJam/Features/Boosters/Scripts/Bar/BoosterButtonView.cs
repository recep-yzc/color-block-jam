using Framework.UI;
using Framework.UI.Buttons;
using Framework.UI.Views;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJam.Boosters
{
    public sealed class BoosterButtonView : UIView
    {
        [Tooltip("Basınca booster'ı kullanan ya da hedef seçmek için kaldıran buton.")]
        [SerializeField] private ActionButton button;
        [Tooltip("Booster'ın ikonu. Booster kalkıkken büyüyüp küçülür.")]
        [SerializeField] private Image icon;
        [Tooltip("Oyuncunun elinde booster varken görünen adet rozeti.")]
        [SerializeField] private GameObject countBadge;
        [Tooltip("Eldeki adedi gösteren yazı.")]
        [SerializeField] private TMP_Text countLabel;
        [Tooltip("Elde hiç kalmadığında görünen, bir kullanımın coin fiyatını gösteren etiket.")]
        [SerializeField] private GameObject priceTag;
        [Tooltip("Bir kullanımın coin fiyatını gösteren yazı.")]
        [SerializeField] private TMP_Text priceLabel;
        [Tooltip("Booster kalkıkken ikonun büyüdüğü en büyük ölçek.")]
        [SerializeField, Min(1f)] private float aimPulseScale = 1.15f;
        [Tooltip("İkonun bir kez büyümesinin ya da küçülmesinin süresi, saniye.")]
        [SerializeField, Min(0.05f)] private float aimPulseDuration = 0.35f;

        private MotionHandle aimPulse;
        private bool isAiming;

        public ActionButton Button => button;

        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        public void SetStock(int count, int price)
        {
            var hasSome = count > 0;
            countBadge.SetActive(hasSome);
            priceTag.SetActive(!hasSome);
            if (hasSome)
            {
                countLabel.SetText("{0}", count);
            }
            else
            {
                priceLabel.SetText("{0}", price);
            }
        }

        public void SetAiming(bool aiming)
        {
            if (aiming == isAiming)
            {
                return;
            }

            isAiming = aiming;
            aimPulse.TryCancel();
            icon.rectTransform.localScale = Vector3.one;
            if (!aiming)
            {
                return;
            }

            aimPulse = LMotion.Create(Vector3.one, Vector3.one * aimPulseScale, aimPulseDuration)
                .WithEase(Ease.InOutSine)
                .WithLoops(-1, LoopType.Yoyo)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(icon.rectTransform)
                .AddTo(this);
        }
    }
}
