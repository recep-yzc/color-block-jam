using Framework.Core.Installers;
using UnityEngine;
using VContainer;

namespace Framework.UI.Popups
{
    public sealed class PopupInstaller : MonoInstaller
    {
        [Tooltip("Popup'ların açıldığı katman.")]
        [SerializeField] private PopupLayer layer;
        [Tooltip("Açılabilecek popup'ların prefab listesi.")]
        [SerializeField] private PopupCatalog catalog;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterPopups(layer, catalog);
        }
    }
}
