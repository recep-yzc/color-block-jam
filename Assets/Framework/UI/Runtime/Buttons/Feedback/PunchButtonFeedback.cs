using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Shrinks while pressed and pops out with a springy punch on release.
    /// </summary>
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Punch", fileName = "PunchButtonFeedback")]
    public sealed class PunchButtonFeedback : ButtonFeedback
    {
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.92f;
        [SerializeField, Min(0.01f)] private float pressDuration = 0.08f;
        [Tooltip("How far past the rest scale the punch goes.")]
        [SerializeField, Range(0f, 1f)] private float punchStrength = 0.18f;
        [SerializeField, Min(0.01f)] private float punchDuration = 0.4f;
        [SerializeField, Min(1)] private int punchFrequency = 4;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale * pressedScale, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = PunchScale(target.Transform, target.RestScale, target.RestScale * punchStrength, punchDuration, punchFrequency);
        }
    }
}
