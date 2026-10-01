using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorBlockJam.UI.Buttons
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ButtonBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("Kapalıyken buton dokunuşa cevap vermez ve soluk görünür.")]
        [SerializeField] private bool interactable = true;
        [Tooltip("Butonun dokunuşa tepkisi. Boş bırakılırsa tepki olmaz.")]
        [SerializeField] private ButtonFeedback feedback;
        [Tooltip("Tepkinin hareket ettirdiği parça. Boş bırakılırsa bütün buton hareket eder.")]
        [SerializeField] private RectTransform feedbackTarget;
        [Tooltip("Buton kapalıyken ne kadar saydam göründüğü. Oyuncu kapalı olduğunu anlasın diye.")]
        [SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.5f;

        private CanvasGroup dimmer;

        private ButtonFeedbackTarget feedbackRest;
        private ButtonFeedbackMotions feedbackMotions;
        private bool isPressed;

        public bool Interactable
        {
            get => interactable;
            set
            {
                if (interactable == value)
                {
                    return;
                }

                interactable = value;
                ShowInteractable();
            }
        }

        protected virtual void Awake()
        {
            feedbackTarget = feedbackTarget != null ? feedbackTarget : (RectTransform)transform;
            feedbackRest = new ButtonFeedbackTarget(feedbackTarget);
            ShowInteractable();
        }

        protected virtual void OnDisable()
        {
            isPressed = false;
            feedbackMotions.CancelAndReset(feedbackRest);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !CanInteract())
            {
                return;
            }

            isPressed = true;
            PlayFeedback(pressed: true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed)
            {
                return;
            }

            isPressed = false;
            PlayFeedback(pressed: false);

            if (eventData.eligibleForClick && !eventData.dragging && CanInteract() && IsOverRestArea(eventData))
            {
                OnClick();
            }
        }

        protected abstract void OnClick();

        private bool IsOverRestArea(PointerEventData eventData)
        {
            var rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera,
                    out var point))
            {
                return false;
            }

            if (feedbackTarget == rect)
            {
                var scale = rect.localScale;
                var rest = feedbackRest.RestScale;
                point = new Vector2(point.x * scale.x / rest.x, point.y * scale.y / rest.y);
                var angle = Mathf.DeltaAngle(feedbackRest.RestAngle, rect.localEulerAngles.z);
                point = Quaternion.Euler(0f, 0f, angle) * point;
            }

            return rect.rect.Contains(point);
        }

        private void ShowInteractable()
        {
            if (dimmer == null)
            {
                if (interactable)
                {
                    return;
                }

                dimmer = gameObject.AddComponent<CanvasGroup>();
            }

            dimmer.alpha = interactable ? 1f : disabledAlpha;
        }

        private bool CanInteract()
        {
            return interactable && isActiveAndEnabled && CanvasGroupInteraction.IsAllowed(transform);
        }

        private void PlayFeedback(bool pressed)
        {
            if (feedback == null)
            {
                return;
            }

            feedbackMotions.Cancel();

            if (pressed)
            {
                feedback.PlayPress(feedbackRest, ref feedbackMotions);
            }
            else
            {
                feedback.PlayRelease(feedbackRest, ref feedbackMotions);
            }
        }
    }
}
