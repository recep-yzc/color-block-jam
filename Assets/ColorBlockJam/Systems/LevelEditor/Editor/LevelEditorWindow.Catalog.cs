using System;
using System.IO;
using ColorBlockJam.Level;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorBlockJam.LevelEditor
{
    internal sealed partial class LevelEditorWindow
    {
        private bool Save(bool asNew)
        {
            var isChecked = result != null && resultRevision == revision && result.IsSolved;
            if ((problems.Count > 0 || !isChecked) && !EditorUtility.DisplayDialog("Level Editor",
                    problems.Count > 0 ? "The level has problems (see Check). Save anyway?" : "The level has not been checked as solvable. Save anyway?",
                    "Save Anyway", "Cancel"))
            {
                return false;
            }

            var isNew = asNew || levelAsset == null;
            var path = isNew ? NextLevelPath() : AssetDatabase.GetAssetPath(levelAsset);
            File.WriteAllText(path, levelJson);
            AssetDatabase.ImportAsset(path);

            if (isNew)
            {
                levelAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                var serializedCatalog = new SerializedObject(catalog);
                var levels = serializedCatalog.FindProperty("levels");
                levels.arraySize++;
                levels.GetArrayElementAtIndex(levels.arraySize - 1).objectReferenceValue = levelAsset;
                serializedCatalog.ApplyModifiedProperties();
            }

            AssetDatabase.SaveAssets();
            IsDirty = false;
            SyncCatalog();
            ShowNotification(new GUIContent($"Saved {Path.GetFileName(path)}"));
            return true;
        }

        private string NextLevelPath()
        {
            var first = FirstLevelIndex();
            var folder = first >= 0
                ? Path.GetDirectoryName(AssetDatabase.GetAssetPath(catalog.Levels[first]))!.Replace('\\', '/')
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
            levelAsset = null;
            catalogIndex = -1;
            IsDirty = true;
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

                try
                {
                    var data = LevelSerializer.FromJson(text.text);
                    levelSummaries.Add($"{i + 1}. {text.name}  ·  {data.difficulty}");
                }
                catch (Exception)
                {
                    levelSummaries.Add($"{i + 1}. {text.name}  (invalid)");
                }
            }
        }

        private int FirstLevelIndex()
        {
            for (var i = 0; i < catalog.Count; i++)
            {
                if (catalog.Levels[i] != null)
                {
                    return i;
                }
            }

            return -1;
        }

        private int IndexOfLevel(TextAsset asset)
        {
            for (var i = 0; i < catalog.Count; i++)
            {
                if (catalog.Levels[i] == asset)
                {
                    return i;
                }
            }

            return -1;
        }

        private Color ColorOf(int index)
        {
            return index >= 0 && index < palette.Count ? palette.GetColor(index) : Color.magenta;
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
