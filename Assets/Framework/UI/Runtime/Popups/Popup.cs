using System;
using Framework.UI.Views;
using UnityEngine;

namespace Framework.UI.Popups
{
    /// <summary>
    /// A modal view opened and closed by <see cref="IPopupService"/>.
    /// Derive from it when the popup has its own content; the service finds popups by their type.
    /// </summary>
    public class Popup : UIView
    {
        [Tooltip("A tap on the dark background closes the popup.")]
        [SerializeField] private bool closeOnBackdropClick = true;
        [Tooltip("The Android back button closes the popup.")]
        [SerializeField] private bool closeOnBackButton = true;

        public event Action<Popup> CloseRequested;

        public bool CloseOnBackdropClick => closeOnBackdropClick;
        public bool CloseOnBackButton => closeOnBackButton;

        /// <summary>Asks the popup service to close this popup, for example from a close button.</summary>
        public void RequestClose()
        {
            CloseRequested?.Invoke(this);
        }
    }
}
