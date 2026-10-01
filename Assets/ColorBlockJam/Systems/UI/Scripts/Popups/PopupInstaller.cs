using ColorBlockJam.Core.Installers;
using UnityEngine;
using VContainer;

namespace ColorBlockJam.UI.Popups
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
