using Framework.Core.Installers;
using UnityEngine;
using VContainer;

namespace Framework.UI.Popups
{
    /// <summary>
    /// Adds the popup service for the popups under <see cref="layer"/> to the scene.
    /// </summary>
    public sealed class PopupInstaller : MonoInstaller
    {
        [SerializeField] private PopupLayer layer;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterPopupLayer(layer);
        }
    }
}
