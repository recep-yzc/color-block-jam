using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Slides a knob between an off and an on position, like a switch.
    /// </summary>
    public sealed class SlideToggleVisual : ToggleStateVisual
    {
        [SerializeField] private RectTransform knob;
        [Tooltip("Knob x position while on, in its parent's space.")]
        [SerializeField] private float onPosition = 40f;
        [Tooltip("Knob x position while off, in its parent's space.")]
        [SerializeField] private float offPosition = -40f;
        [SerializeField, Min(0.01f)] private float duration = 0.25f;
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
