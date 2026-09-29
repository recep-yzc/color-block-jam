using System;

namespace ColorBlockJam.Shared.UI.Buttons
{
    /// <summary>
    /// A button whose click is handled by code, usually a presenter.
    /// </summary>
    public sealed class ActionButton : ButtonBase
    {
        public event Action Clicked;

        protected override void OnClick()
        {
            Clicked?.Invoke();
        }
    }
}
