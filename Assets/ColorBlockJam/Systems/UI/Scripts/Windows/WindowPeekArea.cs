using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorBlockJam.UI.Windows
{
    public sealed class WindowPeekArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("Basılı tutulurken pencere katmanının saydamlığı. 0 = arkadaki oyun tamamen görünür.")]
        [SerializeField, Range(0f, 1f)] private float peekAlpha;
        [Tooltip("Saydamlaşma ve geri gelme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.15f;

        private CanvasGroup layerGroup;
        private MotionHandle fade;
        private bool isPeeking;

        private void Awake()
        {
            var layer = GetComponentInParent<WindowLayer>(true);
            layerGroup = layer != null ? layer.Group : null;

            if (layerGroup == null)
            {
                Debug.LogError($"{name} is not under a {nameof(WindowLayer)} with a group to fade.", this);
            }
        }

        private void OnDisable()
        {
            if (!isPeeking)
            {
                return;
            }

            isPeeking = false;
            fade.TryCancel();
            layerGroup.alpha = 1f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                SetPeeking(true);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetPeeking(false);
        }

        private void SetPeeking(bool value)
        {
            if (isPeeking == value || layerGroup == null)
            {
                return;
            }

            isPeeking = value;
            fade.TryCancel();
            fade = LMotion.Create(layerGroup.alpha, value ? peekAlpha : 1f, fadeDuration)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAlpha(layerGroup)
                .AddTo(this);
        }
    }
}
