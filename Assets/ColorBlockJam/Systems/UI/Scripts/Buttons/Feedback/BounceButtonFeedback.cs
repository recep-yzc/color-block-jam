using LitMotion;
using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    [CreateAssetMenu(menuName = "Color Block Jam/UI/Button Feedback/Bounce", fileName = "BounceButtonFeedback")]
    public sealed class BounceButtonFeedback : ButtonFeedback
    {
        [Tooltip("Buton basılıyken ne kadar aşağı çöktüğü, canvas birimi.")]
        [SerializeField, Min(0f)] private float sinkDistance = 10f;
        [Tooltip("Basılıyken ölçek, normal ölçeğe oranla.")]
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.96f;
        [Tooltip("Basılma hareketinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float pressDuration = 0.08f;
        [Tooltip("Bırakınca zıplamanın yüksekliği, canvas birimi.")]
        [SerializeField, Min(0f)] private float bounceHeight = 22f;
        [Tooltip("Zıplamanın süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float bounceDuration = 0.45f;
        [Tooltip("Zıplama boyunca sekme sayısı.")]
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
