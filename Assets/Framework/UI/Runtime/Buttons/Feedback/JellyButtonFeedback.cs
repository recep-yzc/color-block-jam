using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Squashes flat while pressed and wobbles like jelly on release.
    /// </summary>
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Jelly", fileName = "JellyButtonFeedback")]
    public sealed class JellyButtonFeedback : ButtonFeedback
    {
        [Tooltip("Scale while pressed, per axis, relative to the rest scale.")]
        [SerializeField] private Vector2 squash = new(1.12f, 0.86f);
        [SerializeField, Min(0.01f)] private float pressDuration = 0.1f;
        [Tooltip("Wobble strength per axis. Opposite signs make it stretch while it shrinks.")]
        [SerializeField] private Vector2 wobble = new(-0.1f, 0.14f);
        [SerializeField, Min(0.01f)] private float wobbleDuration = 0.5f;
        [SerializeField, Min(1)] private int wobbleFrequency = 5;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            var scale = Vector3.Scale(target.RestScale, new Vector3(squash.x, squash.y, 1f));
            motions.Scale = ScaleTo(target.Transform, scale, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            var strength = Vector3.Scale(target.RestScale, new Vector3(wobble.x, wobble.y, 0f));
            motions.Scale = PunchScale(target.Transform, target.RestScale, strength, wobbleDuration, wobbleFrequency);
        }
    }
}
