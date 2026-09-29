using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Shared.UI.Popups
{
    public static class PopupContainerBuilderExtensions
    {
        /// <summary>
        /// Registers the popup service and every popup under <paramref name="layer"/> by its own type,
        /// so presenters can ask for their popup directly.
        /// </summary>
        public static void RegisterPopupLayer(this IContainerBuilder builder, PopupLayer layer)
        {
            builder.RegisterComponent(layer);
            builder.RegisterEntryPoint<PopupService>().As<IPopupService>();

            foreach (var popup in layer.Popups)
            {
                builder.RegisterInstance(popup).As(popup.GetType());
            }
        }
    }
}
