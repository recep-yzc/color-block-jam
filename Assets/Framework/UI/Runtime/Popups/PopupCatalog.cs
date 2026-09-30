using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.UI.Popups
{
    /// <summary>
    /// The popup prefabs a scene can open. The popup service instantiates a prefab the first time it opens.
    /// </summary>
    [CreateAssetMenu(menuName = "Framework/UI/Popup Catalog", fileName = "PopupCatalog")]
    public sealed class PopupCatalog : ScriptableObject
    {
        [Tooltip("Her popup türü için bir prefab.")]
        [SerializeField] private Popup[] popups = { };

        [NonSerialized] private Dictionary<Type, Popup> prefabsByType;

        public Popup GetPrefab(Type popupType)
        {
            prefabsByType ??= BuildLookup();

            if (prefabsByType.TryGetValue(popupType, out var prefab))
            {
                return prefab;
            }

            throw new InvalidOperationException($"{name} has no {popupType.Name} prefab. Add it to the catalog.");
        }

        private void OnEnable()
        {
            prefabsByType = null;
        }

        private Dictionary<Type, Popup> BuildLookup()
        {
            var lookup = new Dictionary<Type, Popup>(popups.Length);
            foreach (var popup in popups)
            {
                lookup.Add(popup.GetType(), popup);
            }

            return lookup;
        }
    }
}
