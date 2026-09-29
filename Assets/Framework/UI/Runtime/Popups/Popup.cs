using System;
using Framework.UI.Views;
using UnityEngine;

namespace Framework.UI.Popups
{
    /// <summary>
    /// A modal view opened and closed by <see cref="IPopupService"/>, which instantiates it from a
    /// <see cref="PopupCatalog"/> the first time it opens. Derive from it when the popup has its own content.
    /// A popup with logic has an InstallerScope on its root whose installer registers its presenter;
    /// the presenter lives as long as the popup instance.
    /// </summary>
    public class Popup : UIView
    {
        [Tooltip("A tap on the dark background closes the popup.")]
        [SerializeField] private bool closeOnBackdropClick = true;
        [Tooltip("The Android back button closes the popup.")]
        [SerializeField] private bool closeOnBackButton = true;
        [Tooltip("Destroy the instance when the popup closes, to free memory for popups that open rarely.")]
        [SerializeField] private bool destroyOnHide;

        public event Action<Popup> CloseRequested;

        public bool CloseOnBackdropClick => closeOnBackdropClick;
        public bool CloseOnBackButton => closeOnBackButton;
        public bool DestroyOnHide => destroyOnHide;

        /// <summary>Asks the popup service to close this popup, for example from a close button.</summary>
        public void RequestClose()
        {
            CloseRequested?.Invoke(this);
        }
    }
}
