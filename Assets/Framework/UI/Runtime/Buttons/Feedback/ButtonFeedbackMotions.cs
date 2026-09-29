using LitMotion;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// The running feedback motions of one button. The button owns it; feedback assets fill it.
    /// </summary>
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

        /// <summary>
        /// Stops the running motions and puts back only what they were changing,
        /// so a position owned by a layout group is left alone.
        /// </summary>
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
