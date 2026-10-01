using ColorBlockJam.Core.Installers;
using ColorBlockJam.UI.Buttons;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Gameplay
{
    public sealed class HomeButtonInstaller : MonoInstaller
    {
        [Tooltip("Basılınca seviyeden çıkıp ana ekrana dönen buton.")]
        [SerializeField] private ActionButton homeButton;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<HomeButtonPresenter>().WithParameter(homeButton);
        }
    }
}
