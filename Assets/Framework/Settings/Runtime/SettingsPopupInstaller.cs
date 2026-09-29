using Framework.Core.Installers;
using VContainer;
using VContainer.Unity;

namespace Framework.Settings
{
    /// <summary>
    /// Makes the <see cref="SettingsPopup"/> of this scene work. The popup itself must be under the
    /// scene's popup layer, which a <see cref="Framework.UI.Popups.PopupInstaller"/> registers.
    /// </summary>
    public sealed class SettingsPopupInstaller : MonoInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SettingsPopupPresenter>();
        }
    }
}
