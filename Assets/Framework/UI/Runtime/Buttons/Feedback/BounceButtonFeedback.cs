using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Sinks while pressed and bounces up on release. Do not use it on buttons placed by a layout group,
    /// which owns their position.
    /// </summary>
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Bounce", fileName = "BounceButtonFeedback")]
    public sealed class BounceButtonFeedback : ButtonFeedback
    {
        [Tooltip("How far the button sinks while pressed, in canvas units.")]
        [SerializeField, Min(0f)] private float sinkDistance = 10f;
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.96f;
        [SerializeField, Min(0.01f)] private float pressDuration = 0.08f;
        [Tooltip("How high the bounce goes, in canvas units.")]
        [SerializeField, Min(0f)] private float bounceHeight = 22f;
        [SerializeField, Min(0.01f)] private float bounceDuration = 0.45f;
        [SerializeField, Min(1)] private int bounceFrequency = 3;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale * pressedScale, pressDuration, Ease.OutQuad);
            motions.Position = MoveYTo(target.Transform, target.RestPosition.y - sinkDistance, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale, pressDuration * 2f, Ease.OutBack);
            motions.Position = PunchY(target.Transform, target.RestPosition.y, bounceHeight, bounceDuration, bounceFrequency);
        }
    }
}
