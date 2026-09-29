using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Framework.UI.Transitions
{
    /// <summary>
    /// Strategy for how a view appears and disappears. Each asset is a reusable, tunable animation,
    /// so a new popup design picks or adds an asset instead of changing view code.
    /// Transitions must not keep per-view state: one asset is shared by many views at the same time.
    /// </summary>
    public abstract class ViewTransition : ScriptableObject
    {
        /// <summary>Animates from the hidden look to the rest values of <paramref name="target"/>.</summary>
        public abstract UniTask ShowAsync(ViewTransitionTarget target, CancellationToken cancellationToken);

        /// <summary>Animates from the current look to the hidden look.</summary>
        public abstract UniTask HideAsync(ViewTransitionTarget target, CancellationToken cancellationToken);
    }
}
