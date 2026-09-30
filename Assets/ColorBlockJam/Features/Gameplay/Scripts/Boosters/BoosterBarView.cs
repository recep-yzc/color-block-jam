using Framework.UI;
using Framework.UI.Buttons;
using Framework.UI.Views;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// The booster buttons under the board. Freeze and hammer work and show their price; the others are placeholder
    /// buttons that need no code. While the hammer is taken up, its icon pulses and a hint asks for a block.
    /// </summary>
    public sealed class BoosterBarView : UIView
    {
        [SerializeField] private ActionButton freezeButton;
        [SerializeField] private TMP_Text freezePriceLabel;
        [SerializeField] private ActionButton hammerButton;
        [SerializeField] private TMP_Text hammerPriceLabel;
        [Tooltip("Pulses while the hammer is taken up.")]
        [SerializeField] private RectTransform hammerIcon;
        [Tooltip("Shown while the hammer is taken up, telling the player to tap a block.")]
        [SerializeField] private GameObject hammerHint;
        [SerializeField, Min(1f)] private float aimPulseScale = 1.15f;
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
