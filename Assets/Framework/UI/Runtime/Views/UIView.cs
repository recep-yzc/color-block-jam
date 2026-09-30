using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Framework.UI.Transitions;
using UnityEngine;

namespace Framework.UI.Views
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIView : MonoBehaviour
    {
        [Tooltip("Geçişlerin hareket ettirdiği parça. Boş bırakılırsa bütün görünüm hareket eder.")]
        [SerializeField] private RectTransform content;
        [Tooltip("Görünürken oynayan geçiş. Boş bırakılırsa anında görünür.")]
        [SerializeField] private ViewTransition showTransition;
        [Tooltip("Gizlenirken oynayan geçiş. Boş bırakılırsa anında gizlenir.")]
        [SerializeField] private ViewTransition hideTransition;

        private CanvasGroup canvasGroup;
        private CancellationTokenSource transitionCancellation;
        private Vector3 restScale;
        private Vector2 restPosition;
        private bool isInitialized;

        public event Action Showing;
        public event Action Shown;
        public event Action Hiding;
        public event Action Hidden;

        public ViewState State { get; private set; }
        public bool IsVisible => State == ViewState.Showing || State == ViewState.Visible;

        protected virtual void Awake()
        {
            EnsureInitialized();
        }

        protected virtual void OnDestroy()
        {
            CancelTransition();
        }

        public async UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            if (IsVisible)
            {
                return;
            }

            var token = BeginTransition(cancellationToken);
            State = ViewState.Showing;
            ResetToRest();
            gameObject.SetActive(true);
            canvasGroup.interactable = false;
            Showing?.Invoke();

            if (await PlayAsync(showTransition, show: true, token))
            {
                return;
            }

            State = ViewState.Visible;
            canvasGroup.interactable = true;
            Shown?.Invoke();
        }

        public async UniTask HideAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            if (!IsVisible)
            {
                return;
            }

            var token = BeginTransition(cancellationToken);
            State = ViewState.Hiding;
            canvasGroup.interactable = false;
            Hiding?.Invoke();

            if (await PlayAsync(hideTransition, show: false, token))
            {
                return;
            }

            State = ViewState.Hidden;
            gameObject.SetActive(false);
            Hidden?.Invoke();
        }

        public void HideImmediate()
        {
            EnsureInitialized();
            CancelTransition();
            State = ViewState.Hidden;
            gameObject.SetActive(false);
        }

        private void EnsureInitialized()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            canvasGroup = GetComponent<CanvasGroup>();
            content = content != null ? content : (RectTransform)transform;
            restScale = content.localScale;
            restPosition = content.anchoredPosition;
            State = gameObject.activeSelf ? ViewState.Visible : ViewState.Hidden;
        }

        private void ResetToRest()
        {
            content.localScale = restScale;
            content.anchoredPosition = restPosition;
            canvasGroup.alpha = 1f;
        }

        private async UniTask<bool> PlayAsync(ViewTransition transition, bool show, CancellationToken token)
        {
            if (transition == null)
            {
                return token.IsCancellationRequested;
            }

            var target = new ViewTransitionTarget(content, canvasGroup, restScale, restPosition);
            var animation = show ? transition.ShowAsync(target, token) : transition.HideAsync(target, token);
            return await animation.SuppressCancellationThrow();
        }

        private CancellationToken BeginTransition(CancellationToken externalToken)
        {
            CancelTransition();
            transitionCancellation = CancellationTokenSource.CreateLinkedTokenSource(externalToken, destroyCancellationToken);
            return transitionCancellation.Token;
        }

        private void CancelTransition()
        {
            if (transitionCancellation == null)
            {
                return;
            }

            transitionCancellation.Cancel();
            transitionCancellation.Dispose();
            transitionCancellation = null;
        }
    }
}
