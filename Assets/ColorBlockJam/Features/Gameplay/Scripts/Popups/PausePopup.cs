using Framework.UI.Buttons;
using Framework.UI.Popups;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class PausePopup : Popup
    {
        [Tooltip("Seviyeyi baştan başlatan buton.")]
        [SerializeField] private ActionButton restartButton;
        [Tooltip("Ana ekrana dönen buton.")]
        [SerializeField] private ActionButton homeButton;

        public ActionButton RestartButton => restartButton;
        public ActionButton HomeButton => homeButton;
    }
}
