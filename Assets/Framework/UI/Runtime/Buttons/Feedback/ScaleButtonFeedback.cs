using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Shrinks while pressed and springs back on release.
    /// </summary>
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Scale", fileName = "ScaleButtonFeedback")]
    public sealed class ScaleButtonFeedback : ButtonFeedback
    {
        [Tooltip("Scale while pressed, relative to the rest scale.")]
        [SerializeField, Range(0.5f, 1.5f)] private float pressedScale = 0.9f;

        [Header("Press")]
        [SerializeField, Min(0.01f)] private float pressDuration = 0.08f;
        [SerializeField] private Ease pressEase = Ease.OutQuad;

        [Header("Release")]
        [SerializeField, Min(0.01f)] private float releaseDuration = 0.25f;
        [SerializeField] private Ease releaseEase = Ease.OutBack;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale * pressedScale, pressDuration, pressEase);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale, releaseDuration, releaseEase);
        }
    }
}
