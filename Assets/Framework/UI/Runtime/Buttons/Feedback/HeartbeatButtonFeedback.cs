using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Shrinks while pressed and beats twice like a heart on release.
    /// </summary>
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Heartbeat", fileName = "HeartbeatButtonFeedback")]
    public sealed class HeartbeatButtonFeedback : ButtonFeedback
    {
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.9f;
        [SerializeField, Min(0.01f)] private float pressDuration = 0.08f;
        [Tooltip("How far past the rest scale each beat goes.")]
        [SerializeField, Range(0f, 1f)] private float beatStrength = 0.14f;
        [SerializeField, Min(0.01f)] private float beatDuration = 0.6f;
        [Tooltip("Number of beats.")]
        [SerializeField, Min(1)] private int beats = 2;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale * pressedScale, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = PunchScale(target.Transform, target.RestScale, target.RestScale * beatStrength, beatDuration, beats * 2);
        }
    }
}
