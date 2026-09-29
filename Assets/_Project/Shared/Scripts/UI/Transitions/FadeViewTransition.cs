using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.Shared.UI.Transitions
{
    [CreateAssetMenu(menuName = "Color Block Jam/UI/Transitions/Fade", fileName = "FadeTransition")]
    public sealed class FadeViewTransition : ViewTransition
    {
        [SerializeField, Min(0.01f)] private float showDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float hideDuration = 0.15f;
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
