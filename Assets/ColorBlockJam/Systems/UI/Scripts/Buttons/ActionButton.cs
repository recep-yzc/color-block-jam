using System;

namespace ColorBlockJam.UI.Buttons
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
