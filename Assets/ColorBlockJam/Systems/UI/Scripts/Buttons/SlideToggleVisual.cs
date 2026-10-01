using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    public sealed class SlideToggleVisual : ToggleStateVisual
    {
        [Tooltip("Açılıp kapanırken sağa sola kayan topuz.")]
        [SerializeField] private RectTransform knob;
        [Tooltip("Açıkken topuzun x konumu, ebeveyninin uzayında.")]
        [SerializeField] private float onPosition = 40f;
        [Tooltip("Kapalıyken topuzun x konumu, ebeveyninin uzayında.")]
        [SerializeField] private float offPosition = -40f;
        [Tooltip("Topuzun kayma süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float duration = 0.25f;
        [Tooltip("Kaymanın eğrisi.")]
        [SerializeField] private Ease ease = Ease.OutBack;

        private MotionHandle slide;

        public override void Apply(bool isOn, bool instant)
        {
            slide.TryCancel();
            var target = isOn ? onPosition : offPosition;

            if (instant)
            {
                knob.anchoredPosition = new Vector2(target, knob.anchoredPosition.y);
                return;
            }

            slide = LMotion.Create(knob.anchoredPosition.x, target, duration)
                .WithEase(ease)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAnchoredPositionX(knob);
        }

        private void OnDestroy()
        {
            slide.TryCancel();
        }
    }
}
