using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Economy
{
    public static class CoinHudContainerBuilderExtensions
    {
        public static void RegisterCoinHud(this IContainerBuilder builder, CoinHudView view)
        {
            builder.RegisterComponent(view);
            builder.RegisterEntryPoint<CoinHudPresenter>();
        }
    }
}
