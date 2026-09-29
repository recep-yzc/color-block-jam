using LitMotion;
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

        private Vector3 restScale;
        private MotionHandle feedbackMotion;
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
                OnInteractableChanged(value);
            }
        }

        protected virtual void Awake()
        {
            feedbackTarget = feedbackTarget != null ? feedbackTarget : (RectTransform)transform;
            restScale = feedbackTarget.localScale;
        }

        protected virtual void OnDisable()
        {
            isPressed = false;
            feedbackMotion.TryCancel();
            feedbackTarget.localScale = restScale;
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

        protected virtual void OnInteractableChanged(bool isInteractable)
        {
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

            feedbackMotion.TryCancel();
            feedbackMotion = pressed
                ? feedback.PlayPress(feedbackTarget, restScale)
                : feedback.PlayRelease(feedbackTarget, restScale);
        }
    }
}
