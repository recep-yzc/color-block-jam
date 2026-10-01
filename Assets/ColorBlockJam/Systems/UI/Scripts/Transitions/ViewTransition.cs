using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ColorBlockJam.UI.Transitions
{
    public abstract class ViewTransition : ScriptableObject
    {
        public abstract UniTask ShowAsync(ViewTransitionTarget target, CancellationToken cancellationToken);

        public abstract UniTask HideAsync(ViewTransitionTarget target, CancellationToken cancellationToken);
    }
}
