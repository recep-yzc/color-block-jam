using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Jelly", fileName = "JellyButtonFeedback")]
    public sealed class JellyButtonFeedback : ButtonFeedback
    {
        [Tooltip("Basılıyken eksen başına ölçek, normal ölçeğe oranla.")]
        [SerializeField] private Vector2 squash = new(1.12f, 0.86f);
        [Tooltip("Basılma hareketinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float pressDuration = 0.1f;
        [Tooltip("Eksen başına sallanma gücü. Zıt işaretler bir eksende uzarken diğerinde kısalmasını sağlar.")]
        [SerializeField] private Vector2 wobble = new(-0.1f, 0.14f);
        [Tooltip("Bırakınca sallanmanın süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float wobbleDuration = 0.5f;
        [Tooltip("Sallanma boyunca salınım sayısı.")]
        [SerializeField, Min(1)] private int wobbleFrequency = 5;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            var scale = Vector3.Scale(target.RestScale, new Vector3(squash.x, squash.y, 1f));
            motions.Scale = ScaleTo(target.Transform, scale, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            var strength = Vector3.Scale(target.RestScale, new Vector3(wobble.x, wobble.y, 0f));
            motions.Scale = PunchScale(target.Transform, target.RestScale, strength, wobbleDuration, wobbleFrequency);
        }
    }
}
