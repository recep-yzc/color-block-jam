using Framework.UI;
using Framework.UI.Buttons;
using Framework.UI.Views;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class BoosterBarView : UIView
    {
        [Tooltip("Süreyi bir süreliğine durduran freeze butonu.")]
        [SerializeField] private ActionButton freezeButton;
        [Tooltip("Freeze'in coin fiyatını gösteren yazı.")]
        [SerializeField] private TMP_Text freezePriceLabel;
        [Tooltip("Seçilen bir bloğu kıran hammer butonu. İlk basış hammer'ı kaldırır, ikinci basış geri koyar.")]
        [SerializeField] private ActionButton hammerButton;
        [Tooltip("Hammer'ın coin fiyatını gösteren yazı.")]
        [SerializeField] private TMP_Text hammerPriceLabel;
        [Tooltip("Hammer kalkıkken büyüyüp küçülen ikon.")]
        [SerializeField] private RectTransform hammerIcon;
        [Tooltip("Hammer kalkıkken oyuncuya bir bloğa dokunmasını söyleyen yazı.")]
        [SerializeField] private GameObject hammerHint;
        [Tooltip("Hammer kalkıkken ikonun büyüdüğü en büyük ölçek.")]
        [SerializeField, Min(1f)] private float aimPulseScale = 1.15f;
        [Tooltip("İkonun bir kez büyümesinin ya da küçülmesinin süresi, saniye.")]
        [SerializeField, Min(0.05f)] private float aimPulseDuration = 0.35f;

        private MotionHandle aimPulse;

        public ActionButton FreezeButton => freezeButton;
        public ActionButton HammerButton => hammerButton;

        public void SetPrices(int freeze, int hammer)
        {
            freezePriceLabel.SetText("{0}", freeze);
            hammerPriceLabel.SetText("{0}", hammer);
        }

        public void SetHammerAiming(bool isAiming)
        {
            hammerHint.SetActive(isAiming);
            aimPulse.TryCancel();
            hammerIcon.localScale = Vector3.one;
            if (!isAiming)
            {
                return;
            }

            aimPulse = LMotion.Create(Vector3.one, Vector3.one * aimPulseScale, aimPulseDuration)
                .WithEase(Ease.InOutSine)
                .WithLoops(-1, LoopType.Yoyo)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(hammerIcon)
                .AddTo(this);
        }
    }
}
