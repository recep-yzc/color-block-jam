using LitMotion;
using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    [CreateAssetMenu(menuName = "Color Block Jam/UI/Button Feedback/Pop", fileName = "PopButtonFeedback")]
    public sealed class PopButtonFeedback : ButtonFeedback
    {
        [Tooltip("Basılıyken büyüdüğü ölçek, normal ölçeğe oranla.")]
        [SerializeField, Range(1f, 1.5f)] private float pressedScale = 1.08f;
        [Tooltip("Basılma hareketinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float pressDuration = 0.1f;
        [Tooltip("Bırakınca normale dönme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float releaseDuration = 0.45f;
        [Tooltip("Normale dönüşün eğrisi.")]
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
