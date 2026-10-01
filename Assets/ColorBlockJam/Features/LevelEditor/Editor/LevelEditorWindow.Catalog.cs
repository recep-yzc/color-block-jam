using System.IO;
using ColorBlockJam.Level;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow
    {
        private void Save(bool asNew)
        {
            var data = level.ToData();
            var isChecked = result != null && resultRevision == revision && result.IsSolved;
            if ((problems.Count > 0 || !isChecked) && !EditorUtility.DisplayDialog("Level Editor",
                    problems.Count > 0 ? "The level has problems (see Check). Save anyway?" : "The level has not been checked as solvable. Save anyway?",
                    "Save Anyway", "Cancel"))
            {
                return;
            }

            string path;
            if (asNew || catalogIndex < 0)
            {
                path = NextLevelPath();
            }
            else
            {
                path = AssetDatabase.GetAssetPath(catalog.Levels[catalogIndex]);
            }

            File.WriteAllText(path, LevelSerializer.ToJson(data));
            AssetDatabase.ImportAsset(path);

            if (asNew || catalogIndex < 0)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                var serializedCatalog = new SerializedObject(catalog);
                var levels = serializedCatalog.FindProperty("levels");
                levels.arraySize++;
                levels.GetArrayElementAtIndex(levels.arraySize - 1).objectReferenceValue = asset;
                serializedCatalog.ApplyModifiedProperties();
                catalogIndex = catalog.Count - 1;
            }

            AssetDatabase.SaveAssets();
            isDirty = false;
            RefreshSummaries();
            ShowNotification(new GUIContent($"Saved {Path.GetFileName(path)}"));
        }

        private string NextLevelPath()
        {
            var folder = catalog.Count > 0
                ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(catalog.Levels[0]))!.Replace('\\', '/')
                : Path.GetDirectoryName(AssetDatabase.GetAssetPath(catalog))!.Replace('\\', '/');
            for (var number = catalog.Count + 1; ; number++)
            {
                var path = $"{folder}/Level{number:000}.json";
                if (!File.Exists(path))
                {
                    return path;
                }
            }
        }

        private void MoveInCatalog(int direction)
        {
            var target = catalogIndex + direction;
            if (target < 0 || target >= catalog.Count)
            {
                return;
            }

            var serializedCatalog = new SerializedObject(catalog);
            serializedCatalog.FindProperty("levels").MoveArrayElement(catalogIndex, target);
            serializedCatalog.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            catalogIndex = target;
            RefreshSummaries();
        }

        private void RemoveFromCatalog()
        {
            if (!EditorUtility.DisplayDialog("Level Editor", $"Take level {catalogIndex + 1} out of the catalog? Its file stays in the project.",
                    "Remove", "Cancel"))
            {
                return;
            }

            var serializedCatalog = new SerializedObject(catalog);
            var levels = serializedCatalog.FindProperty("levels");
            levels.GetArrayElementAtIndex(catalogIndex).objectReferenceValue = null;
            levels.DeleteArrayElementAtIndex(catalogIndex);
            serializedCatalog.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            catalogIndex = -1;
            isDirty = true;
            RefreshSummaries();
        }

        private void RefreshSummaries()
        {
            isLayoutStale = true;
            levelSummaries.Clear();
            if (catalog == null)
            {
                return;
            }

            for (var i = 0; i < catalog.Count; i++)
            {
                var text = catalog.Levels[i];
                if (text == null)
                {
                    levelSummaries.Add($"{i + 1}. (missing)");
                    continue;
                }

                var data = LevelSerializer.FromJson(text.text);
                levelSummaries.Add($"{i + 1}. {text.name}  ·  {data.difficulty}");
            }
        }

        private string[] ColorNames()
        {
            var names = new string[palette.Count];
            for (var i = 0; i < names.Length; i++)
            {
                names[i] = palette.GetName(i);
            }

            return names;
        }

        private static EditableLevel NewLevel() => new(LevelData.DefaultWidth, LevelData.DefaultHeight);

        private static T FindAsset<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
