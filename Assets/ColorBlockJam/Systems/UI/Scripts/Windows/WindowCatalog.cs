using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBlockJam.UI.Windows
{
    [CreateAssetMenu(menuName = "Color Block Jam/UI/Window Catalog", fileName = "WindowCatalog")]
    public sealed class WindowCatalog : ScriptableObject
    {
        [Tooltip("Açılabilen her pencere: hangi presenter hangi prefab'ı hangi ayarlarla gösterir.")]
        [SerializeField] private WindowEntry[] windows = { };

        [NonSerialized] private Dictionary<Type, WindowEntry> entriesByPresenter;

        public IReadOnlyList<WindowEntry> Windows => windows;

        public bool TryGetEntry(Type presenterType, out WindowEntry entry)
        {
            entriesByPresenter ??= BuildLookup();
            return entriesByPresenter.TryGetValue(presenterType, out entry);
        }

        private void OnEnable()
        {
            entriesByPresenter = null;
        }

        private Dictionary<Type, WindowEntry> BuildLookup()
        {
            var lookup = new Dictionary<Type, WindowEntry>(windows.Length);
            foreach (var entry in windows)
            {
                var presenterType = entry.PresenterType;
                if (presenterType != null)
                {
                    lookup[presenterType] = entry;
                }
            }

            return lookup;
        }
    }
}
