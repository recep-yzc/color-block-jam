using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Base of every button. It handles touch input, the interactable state and the touch feedback;
    /// derived buttons only decide what a click does.
    /// The button needs a raycast target graphic on its own GameObject.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class ButtonBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private bool interactable = true;
        [Tooltip("How the button reacts to touch. Leave empty for no feedback.")]
        [SerializeField] private ButtonFeedback feedback;
        [Tooltip("The part the feedback moves. Leave empty to move the whole button.")]
        [SerializeField] private RectTransform feedbackTarget;
        [Tooltip("How see-through the button is while it is not interactable, so the player can tell it is off.")]
        [SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.5f;

        // Added the first time the button is switched off, so buttons that never are need no component.
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
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !CanInteract())
            {
                return;
            }

            OnClick();
        }

        protected abstract void OnClick();

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
