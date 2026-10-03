using System;
using System.Collections.Generic;
using System.Linq;
using ColorBlockJam.UI.Windows;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.UI.Editor
{
    [CustomPropertyDrawer(typeof(WindowPresenterTypeAttribute))]
    public sealed class WindowPresenterTypeDrawer : PropertyDrawer
    {
        private const string None = "(none)";
        private const string MissingSuffix = " (missing)";

        private static Type[] presenterTypes;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            presenterTypes ??= TypeCache.GetTypesDerivedFrom<IWindowPresenter>()
                .Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition)
                .OrderBy(type => type.Name)
                .ToArray();

            var options = new List<string> { None };
            options.AddRange(presenterTypes.Select(type => type.Name));
            var selectedIndex = string.IsNullOrEmpty(property.stringValue)
                ? 0
                : Array.FindIndex(presenterTypes, type => NameOf(type) == property.stringValue) + 1;
            if (selectedIndex == 0 && !string.IsNullOrEmpty(property.stringValue))
            {
                options.Add(property.stringValue + MissingSuffix);
                selectedIndex = options.Count - 1;
            }

            using (var scope = new EditorGUI.PropertyScope(position, label, property))
            {
                var newIndex = EditorGUI.Popup(position, scope.content, selectedIndex, options.Select(option => new GUIContent(option)).ToArray());
                if (newIndex != selectedIndex && newIndex <= presenterTypes.Length)
                {
                    property.stringValue = newIndex == 0 ? string.Empty : NameOf(presenterTypes[newIndex - 1]);
                }
            }
        }

        public static string NameOf(Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }
    }
}
