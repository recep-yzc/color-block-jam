using ColorBlockJam.Level;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    [CustomEditor(typeof(LevelCatalog))]
    internal sealed class LevelCatalogEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Levels are made and changed in the level editor. The order here is the play order.", MessageType.Info);
            if (GUILayout.Button("Open Level Editor", GUILayout.Height(28f)))
            {
                LevelEditorWindow.Open();
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceId) is not TextAsset text)
            {
                return false;
            }

            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(LevelCatalog)}"))
            {
                var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(AssetDatabase.GUIDToAssetPath(guid));
                for (var i = 0; i < catalog.Count; i++)
                {
                    if (catalog.Levels[i] == text)
                    {
                        LevelEditorWindow.Open().Load(catalog, i);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
