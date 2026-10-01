using UnityEngine;

namespace ColorBlockJam.UI.Transitions
{
    public readonly struct ViewTransitionTarget
    {
        public readonly RectTransform Content;
        public readonly CanvasGroup CanvasGroup;
        public readonly Vector3 RestScale;

        public ViewTransitionTarget(RectTransform content, CanvasGroup canvasGroup, Vector3 restScale)
        {
            Content = content;
            CanvasGroup = canvasGroup;
            RestScale = restScale;
        }
    }
}
