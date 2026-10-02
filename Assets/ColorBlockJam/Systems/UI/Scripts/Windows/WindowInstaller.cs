using ColorBlockJam.Core.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ColorBlockJam.UI.Windows
{
    [CreateAssetMenu(menuName = "Color Block Jam/Installers/Windows", fileName = "WindowInstaller")]
    public sealed class WindowInstaller : ScriptableInstaller
    {
        [Tooltip("Açılabilecek pencerelerin kataloğu.")]
        [SerializeField] private WindowCatalog catalog;
        [Tooltip("Pencerelerin açıldığı, sahneler arasında yaşayan katman.")]
        [SerializeField] private WindowLayer layer;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(catalog);
            builder.RegisterEntryPoint<WindowService>().WithParameter(layer).As<IWindows>();

            foreach (var entry in catalog.Windows)
            {
                var presenterType = entry.PresenterType;
                if (presenterType == null)
                {
                    Debug.LogError($"{catalog.name} has a window whose presenter '{entry.presenter}' was not found.", catalog);
                    continue;
                }

                builder.Register(presenterType, Lifetime.Singleton);
            }
        }
    }
}
