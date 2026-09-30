using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Tilt", fileName = "TiltButtonFeedback")]
    public sealed class TiltButtonFeedback : ButtonFeedback
    {
        [Tooltip("Basılıyken ölçek, normal ölçeğe oranla.")]
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.95f;
        [Tooltip("Basılıyken eğilme açısı, derece.")]
        [SerializeField] private float tiltAngle = -10f;
        [Tooltip("Basılma hareketinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float pressDuration = 0.1f;
        [Tooltip("Bırakınca düzelme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float releaseDuration = 0.35f;
        [Tooltip("Düzelmenin eğrisi.")]
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
