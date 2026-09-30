using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    public struct ButtonFeedbackMotions
    {
        public MotionHandle Scale;
        public MotionHandle Rotation;
        public MotionHandle Position;

        public void Cancel()
        {
            Scale.TryCancel();
            Rotation.TryCancel();
            Position.TryCancel();
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

            if (Position.TryCancel())
            {
                target.Transform.anchoredPosition = target.RestPosition;
            }
        }
    }
}
