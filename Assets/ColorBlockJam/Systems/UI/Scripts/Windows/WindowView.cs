using System;
using ColorBlockJam.UI.Transitions;
using ColorBlockJam.UI.Views;

namespace ColorBlockJam.UI.Windows
{
    public abstract class WindowView : ViewBase
    {
        private ViewTransition showTransition;
        private ViewTransition hideTransition;

        public event Action<WindowView> CloseRequested;

        protected override ViewTransition ShowTransition => showTransition;
        protected override ViewTransition HideTransition => hideTransition;

        public void UseTransitions(ViewTransition show, ViewTransition hide)
        {
            showTransition = show;
            hideTransition = hide;
        }

        public void RequestClose()
        {
            CloseRequested?.Invoke(this);
        }
    }
}
