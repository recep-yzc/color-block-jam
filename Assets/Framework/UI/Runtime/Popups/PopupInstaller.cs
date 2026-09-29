using Framework.Core.Installers;
using UnityEngine;
using VContainer;

namespace Framework.UI.Popups
{
    /// <summary>
    /// Adds the popup service to the scene. Popups from <see cref="catalog"/> open under <see cref="layer"/>.
    /// </summary>
    public sealed class PopupInstaller : MonoInstaller
    {
        [SerializeField] private PopupLayer layer;
        [SerializeField] private PopupCatalog catalog;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterPopups(layer, catalog);
        }
    }
}
