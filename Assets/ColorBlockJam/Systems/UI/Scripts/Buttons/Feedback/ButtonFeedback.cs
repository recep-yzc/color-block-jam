using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    public abstract class ButtonFeedback : ScriptableObject
    {
        public abstract void PlayPress(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions);

        public abstract void PlayRelease(in ButtonFeedbackTarget target, ref ButtonFeedbackMotions motions);

        protected static MotionHandle ScaleTo(RectTransform target, Vector3 scale, float duration, Ease ease)
        {
            return LMotion.Create(target.localScale, scale, duration)
                .WithEase(ease)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(target);
        }

        protected static MotionHandle RotateTo(RectTransform target, float angle, float duration, Ease ease)
        {
            return LMotion.Create(Mathf.DeltaAngle(0f, target.localEulerAngles.z), angle, duration)
                .WithEase(ease)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalEulerAnglesZ(target);
        }

        protected static MotionHandle PunchScale(RectTransform target, Vector3 rest, Vector3 strength, float duration, int frequency)
        {
            return LMotion.Punch.Create(rest, strength, duration)
                .WithFrequency(frequency)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(target);
        }

        protected static MotionHandle PunchAngle(RectTransform target, float rest, float strength, float duration, int frequency)
        {
            return LMotion.Punch.Create(rest, strength, duration)
                .WithFrequency(frequency)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalEulerAnglesZ(target);
        }
    }
}
