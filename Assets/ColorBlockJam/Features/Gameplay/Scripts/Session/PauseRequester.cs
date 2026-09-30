using System;
using Cysharp.Threading.Tasks;
using Framework.UI.Popups;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Opens the pause popup for everything that should pause the level: the HUD's pause button, the Android back
    /// button when no popup is open, and the app losing focus, for example to a call, the notification shade or the
    /// home button. It only pauses a level being played, so it never covers a result popup.
    /// </summary>
    public sealed class PauseRequester : IStartable, IDisposable
    {
        private readonly LevelSession session;
        private readonly IPopupService popups;

        public PauseRequester(LevelSession session, IPopupService popups)
        {
            this.session = session;
            this.popups = popups;
        }

        public void Start()
        {
            popups.BackPressedWithoutPopup += Request;
            Application.focusChanged += OnFocusChanged;
        }

        public void Dispose()
        {
            popups.BackPressedWithoutPopup -= Request;
            Application.focusChanged -= OnFocusChanged;
        }

        public void Request()
        {
            if (session.State == LevelState.Playing && !popups.HasOpenPopup)
            {
                popups.ShowAsync<PausePopup>().Forget();
            }
        }

        private void OnFocusChanged(bool hasFocus)
        {
            if (!hasFocus)
            {
                Request();
            }
        }
    }
}
