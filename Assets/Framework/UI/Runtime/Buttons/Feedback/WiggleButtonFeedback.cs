using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Wiggle", fileName = "WiggleButtonFeedback")]
    public sealed class WiggleButtonFeedback : ButtonFeedback
    {
        [Tooltip("Basılıyken ölçek, normal ölçeğe oranla.")]
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.9f;
        [Tooltip("Basılma hareketinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float pressDuration = 0.08f;
        [Tooltip("Bırakınca ölçeğin normale dönme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float releaseDuration = 0.25f;
        [Tooltip("En büyük sallanma açısı, derece.")]
        [SerializeField] private float wiggleAngle = 14f;
        [Tooltip("Sallanmanın süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float wiggleDuration = 0.5f;
        [Tooltip("Sallanma boyunca salınım sayısı.")]
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
