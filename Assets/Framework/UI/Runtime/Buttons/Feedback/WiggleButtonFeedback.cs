using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Shrinks while pressed and wiggles side to side on release, like a buzzing phone.
    /// </summary>
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Wiggle", fileName = "WiggleButtonFeedback")]
    public sealed class WiggleButtonFeedback : ButtonFeedback
    {
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.9f;
        [SerializeField, Min(0.01f)] private float pressDuration = 0.08f;
        [SerializeField, Min(0.01f)] private float releaseDuration = 0.25f;
        [Tooltip("Largest wiggle angle in degrees.")]
        [SerializeField] private float wiggleAngle = 14f;
        [SerializeField, Min(0.01f)] private float wiggleDuration = 0.5f;
        [SerializeField, Min(1)] private int wiggleFrequency = 7;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale * pressedScale, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale, releaseDuration, Ease.OutBack);
            motions.Rotation = PunchAngle(target.Transform, target.RestAngle, wiggleAngle, wiggleDuration, wiggleFrequency);
        }
    }
}
