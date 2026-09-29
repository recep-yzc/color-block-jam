using LitMotion;
using UnityEngine;

namespace ColorBlockJam.Shared.UI.Buttons
{
    /// <summary>
    /// Strategy for how a button reacts to touch. One asset is shared by many buttons,
    /// so it must not keep per-button state; the button keeps the returned motion.
    /// </summary>
    public abstract class ButtonFeedback : ScriptableObject
    {
        public abstract MotionHandle PlayPress(RectTransform target, Vector3 restScale);

        public abstract MotionHandle PlayRelease(RectTransform target, Vector3 restScale);
    }
}
