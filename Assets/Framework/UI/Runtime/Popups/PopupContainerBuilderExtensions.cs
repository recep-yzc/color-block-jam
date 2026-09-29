using VContainer;
using VContainer.Unity;

namespace Framework.UI.Popups
{
    public static class PopupContainerBuilderExtensions
    {
        /// <summary>
        /// Registers the popup service that opens the popups of <paramref name="catalog"/> under <paramref name="layer"/>.
        /// </summary>
        public static void RegisterPopups(this IContainerBuilder builder, PopupLayer layer, PopupCatalog catalog)
        {
            builder.RegisterComponent(layer);
            builder.RegisterInstance(catalog);
            builder.RegisterEntryPoint<PopupService>().As<IPopupService>();
        }
    }
}
