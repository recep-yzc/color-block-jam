using Framework.Core.Installers;
using Framework.UI.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Framework.UI.Popups
{
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
        }
    }
}
