using Framework.Core.Installers;
using Framework.UI.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Framework.UI.Popups
{
    /// <summary>
    /// Base of the installer put on a popup prefab, next to its InstallerScope, that gives the popup its presenter.
    /// A popup needs only an empty subclass:
    /// <c>public sealed class PausePopupInstaller : PopupPresenterInstaller&lt;PausePopup, PausePopupPresenter&gt; { }</c>
    /// </summary>
    public abstract class PopupPresenterInstaller<TPopup, TPresenter> : MonoInstaller
        where TPopup : Popup
        where TPresenter : ViewPresenter<TPopup>
    {
        public override void Install(IContainerBuilder builder)
        {
            var popup = GetComponent<TPopup>();
            if (popup == null)
            {
                Debug.LogError($"{name} has no {typeof(TPopup).Name} next to its installer.", this);
                return;
            }

            builder.RegisterComponent(popup);
            builder.RegisterEntryPoint<TPresenter>();
            InstallMore(builder);
        }

        /// <summary>Registers anything else only this popup uses.</summary>
        protected virtual void InstallMore(IContainerBuilder builder)
        {
        }
    }
}
