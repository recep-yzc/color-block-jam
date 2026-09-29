using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Framework.UI.Buttons
{
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

        public override MotionHandle PlayPress(RectTransform target, Vector3 restScale)
        {
            return LMotion.Create(target.localScale, restScale * pressedScale, pressDuration)
                .WithEase(pressEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(target);
        }

        public override MotionHandle PlayRelease(RectTransform target, Vector3 restScale)
        {
            return LMotion.Create(target.localScale, restScale, releaseDuration)
                .WithEase(releaseEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(target);
        }
    }
}
