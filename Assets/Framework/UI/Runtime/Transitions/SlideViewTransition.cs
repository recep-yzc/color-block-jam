using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Framework.UI.Transitions
{
    [CreateAssetMenu(menuName = "Framework/UI/Transitions/Slide", fileName = "SlideTransition")]
    public sealed class SlideViewTransition : ViewTransition
    {
        private enum Side
        {
            Top,
            Bottom,
            Left,
            Right
        }

        [Tooltip("The side the view comes in from and goes out to.")]
        [SerializeField] private Side side = Side.Bottom;
        [Tooltip("How far the view moves, in canvas units.")]
        [SerializeField, Min(0f)] private float distance = 600f;
        [SerializeField] private bool fade = true;

        [Header("Show")]
        [SerializeField, Min(0.01f)] private float showDuration = 0.3f;
        [SerializeField] private Ease showEase = Ease.OutCubic;

        [Header("Hide")]
        [SerializeField, Min(0.01f)] private float hideDuration = 0.2f;
        [SerializeField] private Ease hideEase = Ease.InCubic;

        public override UniTask ShowAsync(ViewTransitionTarget target, CancellationToken cancellationToken)
        {
            var move = LMotion.Create(target.RestPosition + HiddenOffset, target.RestPosition, showDuration)
                .WithEase(showEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAnchoredPosition(target.Content)
                .ToUniTask(cancellationToken);

            if (!fade)
            {
                return move;
            }

            var alpha = LMotion.Create(0f, 1f, showDuration)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAlpha(target.CanvasGroup)
                .ToUniTask(cancellationToken);

            return UniTask.WhenAll(move, alpha);
        }

        public override UniTask HideAsync(ViewTransitionTarget target, CancellationToken cancellationToken)
        {
            var move = LMotion.Create(target.Content.anchoredPosition, target.RestPosition + HiddenOffset, hideDuration)
                .WithEase(hideEase)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAnchoredPosition(target.Content)
                .ToUniTask(cancellationToken);

            if (!fade)
            {
                return move;
            }

            var alpha = LMotion.Create(target.CanvasGroup.alpha, 0f, hideDuration)
                .WithScheduler(UIMotion.Scheduler)
                .BindToAlpha(target.CanvasGroup)
                .ToUniTask(cancellationToken);

            return UniTask.WhenAll(move, alpha);
        }

        private Vector2 HiddenOffset => side switch
        {
            Side.Top => new Vector2(0f, distance),
            Side.Bottom => new Vector2(0f, -distance),
            Side.Left => new Vector2(-distance, 0f),
            _ => new Vector2(distance, 0f)
        };
    }
}
