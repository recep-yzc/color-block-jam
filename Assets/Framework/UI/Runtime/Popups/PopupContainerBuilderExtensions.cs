using VContainer;
using VContainer.Unity;

namespace Framework.UI.Popups
{
    public static class PopupContainerBuilderExtensions
    {
        public static void RegisterPopups(this IContainerBuilder builder, PopupLayer layer, PopupCatalog catalog)
        {
            builder.RegisterComponent(layer);
            builder.RegisterInstance(catalog);
            builder.RegisterEntryPoint<PopupService>().As<IPopupService>();
        }
    }
}
