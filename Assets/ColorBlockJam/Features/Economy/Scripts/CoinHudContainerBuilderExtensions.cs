using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.Economy
{
    public static class CoinHudContainerBuilderExtensions
    {
        /// <summary>
        /// Makes a CoinHud instance in the scene show the wallet.
        /// </summary>
        public static void RegisterCoinHud(this IContainerBuilder builder, CoinHudView view)
        {
            builder.RegisterComponent(view);
            builder.RegisterEntryPoint<CoinHudPresenter>();
        }
    }
}
