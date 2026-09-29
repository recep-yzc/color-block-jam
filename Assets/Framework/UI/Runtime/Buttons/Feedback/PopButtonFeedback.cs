using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Grows while pressed, the opposite of a squash, and settles back elastically on release.
    /// </summary>
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Pop", fileName = "PopButtonFeedback")]
    public sealed class PopButtonFeedback : ButtonFeedback
    {
        [SerializeField, Range(1f, 1.5f)] private float pressedScale = 1.08f;
        [SerializeField, Min(0.01f)] private float pressDuration = 0.1f;
        [SerializeField, Min(0.01f)] private float releaseDuration = 0.45f;
        [SerializeField] private Ease releaseEase = Ease.OutElastic;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale * pressedScale, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale, releaseDuration, releaseEase);
        }
    }
}
