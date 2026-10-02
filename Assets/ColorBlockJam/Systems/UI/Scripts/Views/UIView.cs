using ColorBlockJam.UI.Transitions;
using UnityEngine;

namespace ColorBlockJam.UI.Views
{
    public abstract class UIView : ViewBase
    {
        [Tooltip("Görünürken oynayan geçiş. Boş bırakılırsa anında görünür.")]
        [SerializeField] private ViewTransition showTransition;
        [Tooltip("Gizlenirken oynayan geçiş. Boş bırakılırsa anında gizlenir.")]
        [SerializeField] private ViewTransition hideTransition;

        protected override ViewTransition ShowTransition => showTransition;
        protected override ViewTransition HideTransition => hideTransition;
    }
}
