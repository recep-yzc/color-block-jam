using System.Threading;
using ColorBlockJam.UI.Transitions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.UI.Views
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ViewBase : MonoBehaviour
    {
        [Tooltip("Geçişlerin hareket ettirdiği parça. Boş bırakılırsa bütün görünüm hareket eder.")]
        [SerializeField] private RectTransform content;

        private CanvasGroup canvasGroup;
        private CancellationTokenSource transitionCancellation;
        private Vector3 restScale;
        private bool isInitialized;

        public ViewState State { get; private set; }

        protected abstract ViewTransition ShowTransition { get; }
        protected abstract ViewTransition HideTransition { get; }
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

            if (await PlayAsync(ShowTransition, show: true, token))
            {
                return;
            }

            State = ViewState.Visible;
            canvasGroup.interactable = true;
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

            if (await PlayAsync(HideTransition, show: false, token))
            {
                return;
            }

            State = ViewState.Hidden;
            gameObject.SetActive(false);
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
            State = gameObject.activeSelf ? ViewState.Visible : ViewState.Hidden;
        }

        private void ResetToRest()
        {
            content.localScale = restScale;
            canvasGroup.alpha = 1f;
        }

        private async UniTask<bool> PlayAsync(ViewTransition transition, bool show, CancellationToken token)
        {
            if (transition == null)
            {
                return token.IsCancellationRequested;
            }

            var target = new ViewTransitionTarget(content, canvasGroup, restScale);
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
