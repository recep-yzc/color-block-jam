using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Tilts and shrinks a little while pressed, swings back past rest on release.
    /// </summary>
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Tilt", fileName = "TiltButtonFeedback")]
    public sealed class TiltButtonFeedback : ButtonFeedback
    {
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.95f;
        [Tooltip("Tilt angle while pressed, in degrees.")]
        [SerializeField] private float tiltAngle = -10f;
        [SerializeField, Min(0.01f)] private float pressDuration = 0.1f;
        [SerializeField, Min(0.01f)] private float releaseDuration = 0.35f;
        [SerializeField] private Ease releaseEase = Ease.OutBack;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale * pressedScale, pressDuration, Ease.OutQuad);
            motions.Rotation = RotateTo(target.Transform, target.RestAngle + tiltAngle, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale, releaseDuration, releaseEase);
            motions.Rotation = RotateTo(target.Transform, target.RestAngle, releaseDuration, releaseEase);
        }
    }
}
