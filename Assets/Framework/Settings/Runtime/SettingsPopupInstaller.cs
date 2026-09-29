using Framework.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Framework.Settings
{
    /// <summary>
    /// Put on the settings popup prefab, next to its InstallerScope. Gives the popup its presenter.
    /// </summary>
    [RequireComponent(typeof(SettingsPopup))]
    public sealed class SettingsPopupInstaller : MonoInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(GetComponent<SettingsPopup>());
            builder.RegisterEntryPoint<SettingsPopupPresenter>();
        }
    }
}
