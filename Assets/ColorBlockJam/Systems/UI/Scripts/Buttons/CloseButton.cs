using ColorBlockJam.UI.Windows;
using UnityEngine;

namespace ColorBlockJam.UI.Buttons
{
    public sealed class CloseButton : ButtonBase
    {
        private WindowView window;

        protected override void Awake()
        {
            base.Awake();
            window = GetComponentInParent<WindowView>(true);

            if (window == null)
            {
                Debug.LogError($"{name} is not under a {nameof(WindowView)}.", this);
            }
        }

        protected override void OnClick()
        {
            window.RequestClose();
        }
    }
}
