using System;

namespace Framework.UI.Buttons
{
    public sealed class ActionButton : ButtonBase
    {
        public event Action Clicked;

        protected override void OnClick()
        {
            Clicked?.Invoke();
        }
    }
}
