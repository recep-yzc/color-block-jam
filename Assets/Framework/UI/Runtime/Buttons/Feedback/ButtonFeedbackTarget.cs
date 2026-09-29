using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// The transform a <see cref="ButtonFeedback"/> animates, with its values at rest.
    /// </summary>
    public readonly struct ButtonFeedbackTarget
    {
        public readonly RectTransform Transform;
        public readonly Vector3 RestScale;
        public readonly float RestAngle;
        public readonly Vector2 RestPosition;

        public ButtonFeedbackTarget(RectTransform transform)
        {
            Transform = transform;
            RestScale = transform.localScale;
            RestAngle = Mathf.DeltaAngle(0f, transform.localEulerAngles.z);
            RestPosition = transform.anchoredPosition;
        }
    }
}
