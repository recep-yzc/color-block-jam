using LitMotion;
using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    [CreateAssetMenu(menuName = "Color Block Jam/UI/Button Feedback/Spin", fileName = "SpinButtonFeedback")]
    public sealed class SpinButtonFeedback : ButtonFeedback
    {
        [Tooltip("Basılıyken ölçek, normal ölçeğe oranla.")]
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.88f;
        [Tooltip("Basılıyken dönüşün tersine kurulma açısı, derece.")]
        [SerializeField] private float windUpAngle = 25f;
        [Tooltip("Basılma hareketinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float pressDuration = 0.1f;
        [Tooltip("Açıkken saat yönünde döner.")]
        [SerializeField] private bool clockwise = true;
        [Tooltip("Bırakınca bir tam turun süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float spinDuration = 0.45f;
        [Tooltip("Dönüşün eğrisi.")]
        [SerializeField] private Ease spinEase = Ease.OutCubic;

        private float Direction => clockwise ? -1f : 1f;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale * pressedScale, pressDuration, Ease.OutQuad);
            motions.Rotation = RotateTo(target.Transform, target.RestAngle - Direction * windUpAngle, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale, spinDuration, Ease.OutBack);
            motions.Rotation = RotateTo(target.Transform, target.RestAngle + Direction * 360f, spinDuration, spinEase);
        }
    }
}
