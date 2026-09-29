using Framework.UI.Popups;
using UnityEngine;

namespace Framework.UI.Buttons
{
    /// <summary>
    /// Closes the popup it is placed in. Needs no wiring: drop it anywhere under a <see cref="Popup"/>.
    /// </summary>
    public sealed class CloseButton : ButtonBase
    {
        private Popup popup;

        protected override void Awake()
        {
            base.Awake();
            popup = GetComponentInParent<Popup>(true);

            if (popup == null)
            {
                Debug.LogError($"{name} is not under a {nameof(Popup)}.", this);
            }
        }

        protected override void OnClick()
        {
            popup.RequestClose();
        }
    }
}
