using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Framework.UI.Transitions
{
    [CreateAssetMenu(menuName = "Framework/UI/Transitions/Fade", fileName = "FadeTransition")]
    public sealed class FadeViewTransition : ViewTransition
    {
        [Tooltip("Görünme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float showDuration = 0.2f;
        [Tooltip("Gizlenme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float hideDuration = 0.15f;
        [Tooltip("Geçişin eğrisi.")]
        [SerializeField] private Ease ease = Ease.Linear;

        public override UniTask ShowAsync(ViewTransitionTarget target, CancellationToken cancellationToken)
        {
            return LMotion.Create(0f, 1f, showDuration)
                .WithEase(ease)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAlpha(target.CanvasGroup)
                .ToUniTask(cancellationToken);
        }

        public override UniTask HideAsync(ViewTransitionTarget target, CancellationToken cancellationToken)
        {
            return LMotion.Create(target.CanvasGroup.alpha, 0f, hideDuration)
                .WithEase(ease)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAlpha(target.CanvasGroup)
                .ToUniTask(cancellationToken);
        }
    }
}
