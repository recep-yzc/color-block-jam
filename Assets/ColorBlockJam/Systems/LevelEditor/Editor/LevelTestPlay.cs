using ColorBlockJam.Core.SceneManagement;
using ColorBlockJam.Level;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ColorBlockJam.LevelEditor
{
    [InitializeOnLoad]
    internal static class LevelTestPlay
    {
        static LevelTestPlay()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static void Play(LevelData level)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = FindGameplayScene();
            if (scene == null)
            {
                EditorUtility.DisplayDialog("Level Editor", $"The {GameScenes.Gameplay} scene is not in the build settings.", "OK");
                return;
            }

            EditorTestLevel.Begin(level);
            EditorSceneManager.playModeStartScene = scene;
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredEditMode || !EditorTestLevel.IsActive)
            {
                return;
            }

            EditorTestLevel.End();
            EditorSceneManager.playModeStartScene = null;
        }

        private static SceneAsset FindGameplayScene()
        {
            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(buildScene.path) == GameScenes.Gameplay)
                {
                    return AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                }
            }

            return null;
        }
    }
}
