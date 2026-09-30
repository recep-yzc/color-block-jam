using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Tints graphics with one color while the toggle is on and another while it is off.
    /// </summary>
    public sealed class ToggleColorVisual : ToggleStateVisual
    {
        [Tooltip("Açık ya da kapalı olmaya göre rengi değişen grafikler.")]
        [SerializeField] private Graphic[] graphics;
        [Tooltip("Açıkken renk.")]
        [SerializeField] private Color onColor = Color.white;
        [Tooltip("Kapalıyken renk.")]
        [SerializeField] private Color offColor = new(0.5f, 0.5f, 0.5f, 1f);
        [Tooltip("Renk geçişinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float duration = 0.15f;

        private MotionHandle[] motions;

        public override void Apply(bool isOn, bool instant)
        {
            motions ??= new MotionHandle[graphics.Length];
            var color = isOn ? onColor : offColor;

            for (var i = 0; i < graphics.Length; i++)
            {
                motions[i].TryCancel();

                if (instant)
                {
                    graphics[i].color = color;
                    continue;
                }

                motions[i] = LMotion.Create(graphics[i].color, color, duration)
                    .WithScheduler(UIMotion.Scheduler)
                    .BindToColor(graphics[i]);
            }
        }

        private void OnDestroy()
        {
            if (motions == null)
            {
                return;
            }

            foreach (var motion in motions)
            {
                motion.TryCancel();
            }
        }
    }
}
