using Framework.UI.Buttons;
using Framework.UI.Popups;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    /// <summary>
    /// Holds the level while it is open. Resume is a close button; restart and home leave the level.
    /// </summary>
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
