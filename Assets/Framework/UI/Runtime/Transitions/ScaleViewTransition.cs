using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Framework.UI.Transitions
{
    [CreateAssetMenu(menuName = "Framework/UI/Transitions/Scale", fileName = "ScaleTransition")]
    public sealed class ScaleViewTransition : ViewTransition
    {
        [Tooltip("Scale of the hidden view, relative to its rest scale.")]
        [SerializeField, Min(0f)] private float hiddenScale = 0.7f;
        [SerializeField] private bool fade = true;

        [Header("Show")]
        [SerializeField, Min(0.01f)] private float showDuration = 0.25f;
        [SerializeField] private Ease showEase = Ease.OutBack;

        [Header("Hide")]
        [SerializeField, Min(0.01f)] private float hideDuration = 0.15f;
        [SerializeField] private Ease hideEase = Ease.InBack;

        public override UniTask ShowAsync(ViewTransitionTarget target, CancellationToken cancellationToken)
        {
            var scale = LMotion.Create(target.RestScale * hiddenScale, target.RestScale, showDuration)
                .WithEase(showEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(target.Content)
                .ToUniTask(cancellationToken);

            if (!fade)
            {
                return scale;
            }

            var alpha = LMotion.Create(0f, 1f, showDuration)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAlpha(target.CanvasGroup)
                .ToUniTask(cancellationToken);

            return UniTask.WhenAll(scale, alpha);
        }

        public override UniTask HideAsync(ViewTransitionTarget target, CancellationToken cancellationToken)
        {
            var scale = LMotion.Create(target.Content.localScale, target.RestScale * hiddenScale, hideDuration)
                .WithEase(hideEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToLocalScale(target.Content)
                .ToUniTask(cancellationToken);

            if (!fade)
            {
                return scale;
            }

            var alpha = LMotion.Create(target.CanvasGroup.alpha, 0f, hideDuration)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAlpha(target.CanvasGroup)
                .ToUniTask(cancellationToken);

            return UniTask.WhenAll(scale, alpha);
        }
    }
}
