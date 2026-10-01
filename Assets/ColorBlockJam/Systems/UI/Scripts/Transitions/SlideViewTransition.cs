using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ColorBlockJam.UI.Transitions
{
    [CreateAssetMenu(menuName = "Color Block Jam/UI/Transitions/Slide", fileName = "SlideTransition")]
    public sealed class SlideViewTransition : ViewTransition
    {
        private enum Side
        {
            Top,
            Bottom,
            Left,
            Right
        }

        [Tooltip("Görünümün girip çıktığı kenar.")]
        [SerializeField] private Side side = Side.Bottom;
        [Tooltip("Görünümün kaydığı mesafe, canvas birimi.")]
        [SerializeField, Min(0f)] private float distance = 600f;
        [Tooltip("Açıkken kaymayla birlikte saydamlık da değişir.")]
        [SerializeField] private bool fade = true;

        [Header("Show")]
        [Tooltip("Görünme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float showDuration = 0.3f;
        [Tooltip("Görünmenin eğrisi.")]
        [SerializeField] private Ease showEase = Ease.OutCubic;

        [Header("Hide")]
        [Tooltip("Gizlenme süresi, saniye.")]
        [SerializeField, Min(0.01f)] private float hideDuration = 0.2f;
        [Tooltip("Gizlenmenin eğrisi.")]
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
