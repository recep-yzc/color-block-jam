using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Navigation.Editor
{
    /// <summary>
    /// Draws a dropdown of the page ids in the project's <see cref="NavigationConfig"/>,
    /// so designers pick an id instead of typing it.
    /// </summary>
    [CustomPropertyDrawer(typeof(NavigationIdAttribute))]
    public sealed class NavigationIdDrawer : PropertyDrawer
    {
        private const string MissingIdSuffix = " (missing)";

        private static NavigationConfig cachedConfig;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var config = FindConfig();
            if (property.propertyType != SerializedPropertyType.String || config == null || config.PageOrder.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var options = new List<string>(config.PageOrder);
            var selectedIndex = options.IndexOf(property.stringValue);
            if (selectedIndex < 0)
            {
                options.Add(property.stringValue + MissingIdSuffix);
                selectedIndex = options.Count - 1;
            }

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var newIndex = EditorGUI.Popup(position, label.text, selectedIndex, options.ToArray());
                if (newIndex != selectedIndex && newIndex < config.PageOrder.Count)
                {
                    property.stringValue = config.PageOrder[newIndex];
                }
            }
        }

        private static NavigationConfig FindConfig()
        {
            if (cachedConfig != null)
            {
                return cachedConfig;
            }

            var guids = AssetDatabase.FindAssets($"t:{nameof(NavigationConfig)}");
            if (guids.Length > 0)
            {
                cachedConfig = AssetDatabase.LoadAssetAtPath<NavigationConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            return cachedConfig;
        }
    }
}
