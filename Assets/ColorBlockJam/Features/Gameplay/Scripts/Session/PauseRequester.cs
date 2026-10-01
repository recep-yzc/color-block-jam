using System;
using Cysharp.Threading.Tasks;
using Framework.UI.Popups;
using UnityEngine;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
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
            if (!Application.isEditor)
            {
                Application.focusChanged += OnFocusChanged;
            }
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
