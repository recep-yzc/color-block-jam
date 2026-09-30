using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    [CreateAssetMenu(menuName = "Framework/UI/Button Feedback/Heartbeat", fileName = "HeartbeatButtonFeedback")]
    public sealed class HeartbeatButtonFeedback : ButtonFeedback
    {
        [Tooltip("Basılıyken ölçek, normal ölçeğe oranla.")]
        [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.9f;
        [Tooltip("Basılma hareketinin süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float pressDuration = 0.08f;
        [Tooltip("Her atışın normal ölçeğin ne kadar ötesine gittiği.")]
        [SerializeField, Range(0f, 1f)] private float beatStrength = 0.14f;
        [Tooltip("Bütün atışların süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float beatDuration = 0.6f;
        [Tooltip("Bırakınca kalbin kaç kez attığı.")]
        [SerializeField, Min(1)] private int beats = 2;

        public override void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = ScaleTo(target.Transform, target.RestScale * pressedScale, pressDuration, Ease.OutQuad);
        }

        public override void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions)
        {
            motions.Scale = PunchScale(target.Transform, target.RestScale, target.RestScale * beatStrength, beatDuration, beats * 2);
        }
    }
}
