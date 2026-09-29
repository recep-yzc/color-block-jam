using UnityEngine;

namespace ColorBlockJam.Shared.UI.Transitions
{
    /// <summary>
    /// What a <see cref="ViewTransition"/> animates, with the values of the fully visible state.
    /// </summary>
    public readonly struct ViewTransitionTarget
    {
        public readonly RectTransform Content;
        public readonly CanvasGroup CanvasGroup;
        public readonly Vector3 RestScale;
        public readonly Vector2 RestPosition;

        public ViewTransitionTarget(RectTransform content, CanvasGroup canvasGroup, Vector3 restScale, Vector2 restPosition)
        {
            Content = content;
            CanvasGroup = canvasGroup;
            RestScale = restScale;
            RestPosition = restPosition;
        }
    }
}
