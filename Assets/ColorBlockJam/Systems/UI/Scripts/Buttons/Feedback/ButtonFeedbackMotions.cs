using LitMotion;
using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    public struct ButtonFeedbackMotions
    {
        public MotionHandle Scale;
        public MotionHandle Rotation;

        public void Cancel()
        {
            Scale.TryCancel();
            Rotation.TryCancel();
        }

        public void CancelAndReset(in ButtonFeedbackTarget target)
        {
            if (Scale.TryCancel())
            {
                target.Transform.localScale = target.RestScale;
            }

            if (Rotation.TryCancel())
            {
                target.Transform.localEulerAngles = new Vector3(0f, 0f, target.RestAngle);
            }
        }
    }
}
