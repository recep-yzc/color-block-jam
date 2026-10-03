using ColorBlockJam.UI.Buttons;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    public sealed class PauseMenu : MonoBehaviour
    {
        [Tooltip("Seviyeden çıkıp ana ekrana dönen buton.")]
        [SerializeField] private ActionButton homeButton;

        public ActionButton HomeButton => homeButton;
    }
}
