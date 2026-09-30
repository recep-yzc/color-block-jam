using UnityEngine;

namespace Framework.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaPadding : MonoBehaviour
    {
        [Tooltip("Açıkken obje ekranın üstündeki çentikten ve durum çubuğundan uzak tutulur.")]
        [SerializeField] private bool avoidTop = true;
        [Tooltip("Açıkken obje ekranın altındaki güvenli olmayan alandan, örneğin hareket çubuğundan, uzak tutulur.")]
        [SerializeField] private bool avoidBottom;

        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private Vector2 restPosition;
        private Rect appliedSafeArea;
        private float appliedScale;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
            restPosition = rectTransform.anchoredPosition;
        }

        private void Update()
        {
            var safeArea = Screen.safeArea;
            var scale = rootCanvas.scaleFactor;
            if (safeArea == appliedSafeArea && Mathf.Approximately(scale, appliedScale))
            {
                return;
            }

            appliedSafeArea = safeArea;
            appliedScale = scale;

            var topInset = avoidTop ? (Screen.height - safeArea.yMax) / scale : 0f;
            var bottomInset = avoidBottom ? safeArea.yMin / scale : 0f;
            rectTransform.anchoredPosition = restPosition + new Vector2(0f, bottomInset - topInset);
        }
    }
}
