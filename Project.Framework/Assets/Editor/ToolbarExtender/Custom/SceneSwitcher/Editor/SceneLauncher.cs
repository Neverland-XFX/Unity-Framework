using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace UnityFramework.SceneLauncher
{
    [InitializeOnLoad]
    public class SceneSwitchLeftButton
    {
        private const string PreviousSceneKey = "UnityFramework_PreviousScenePath"; // 用于存储之前场景路径的键
        private const string IsLauncherBtn = "UnityFramework_IsLauncher"; // 用于存储之前是否按下launcher

        private static readonly string SceneMain = "main";

        private static readonly string ButtonStyleName = "Tab middle";
        private static GUIStyle _buttonGuiStyle;

        static SceneSwitchLeftButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuit;
        }

        private static void OnToolbarGUI()
        {
            _buttonGuiStyle ??= new GUIStyle(ButtonStyleName)
            {
                padding = new RectOffset(2, 8, 2, 2),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(
                    new GUIContent("Launcher", EditorGUIUtility.FindTexture("PlayButton"), "Start Scene Launcher"),
                    _buttonGuiStyle))
                SceneHelper.StartScene(SceneMain);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // 从 EditorPrefs 读取之前的场景路径
                var previousScenePath = EditorPrefs.GetString(PreviousSceneKey, string.Empty);
                if (!string.IsNullOrEmpty(previousScenePath) && EditorPrefs.GetBool(IsLauncherBtn))
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            EditorSceneManager.OpenScene(previousScenePath);
                    };
                }

                EditorPrefs.SetBool(IsLauncherBtn, false);
            }
        }

        private static void OnEditorQuit()
        {
            EditorPrefs.SetString(PreviousSceneKey, "");
            EditorPrefs.SetBool(IsLauncherBtn, false);
        }

        private static class SceneHelper
        {
            private static string _sceneToOpen;

            public static void StartScene(string sceneName)
            {
                if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;

                // 记录当前场景路径到 EditorPrefs
                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.isLoaded && activeScene.name != SceneMain)
                {
                    EditorPrefs.SetString(PreviousSceneKey, activeScene.path);
                    EditorPrefs.SetBool(IsLauncherBtn, true);
                }

                _sceneToOpen = sceneName;
                EditorApplication.update += OnUpdate;
            }

            private static void OnUpdate()
            {
                if (_sceneToOpen == null ||
                    EditorApplication.isPlaying || EditorApplication.isPaused ||
                    EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                EditorApplication.update -= OnUpdate;

                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    string[] guids = AssetDatabase.FindAssets("t:scene " + _sceneToOpen, null);
                    if (guids.Length == 0)
                    {
                        Debug.LogWarning("Couldn't find scene file");
                    }
                    else
                    {
                        string scenePath = null;
                        // 优先打开完全匹配_sceneToOpen的场景
                        for (var i = 0; i < guids.Length; i++)
                        {
                            scenePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                            if (scenePath.EndsWith("/" + _sceneToOpen + ".unity")) break;
                        }

                        // 如果没有完全匹配的场景，默认显示找到的第一个场景
                        if (string.IsNullOrEmpty(scenePath)) scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);

                        EditorSceneManager.OpenScene(scenePath);
                        EditorApplication.isPlaying = true;
                    }
                }

                _sceneToOpen = null;
            }
        }
    }
}