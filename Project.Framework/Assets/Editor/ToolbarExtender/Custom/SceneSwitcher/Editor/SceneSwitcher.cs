#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace UnityToolbarExtender.Examples
{
    [InitializeOnLoad]
    public sealed class SceneSwitchLeftButton
    {
        private static List<(string sceneName, string scenePath)> m_Scenes;
        private static string[] m_SceneName;
        private static string[] m_ScenePath;
        private static int sceneSelected = 0;

        static SceneSwitchLeftButton()
        {
            EditorApplication.projectChanged += UpdateCurrent;
            UpdateCurrent();
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        static void UpdateCurrent()
        {
            m_Scenes = SceneSwitcher.GetAllScenesInProject();
            m_SceneName = new string[m_Scenes.Count];
            m_ScenePath = new string[m_Scenes.Count];
            for (int i = 0; i < m_Scenes.Count; i++)
            {
                var (name, path) = m_Scenes[i];
                m_SceneName[i] = name;
                m_ScenePath[i] = path;
                if (SceneManager.GetActiveScene().path == path)
                    sceneSelected = i;
            }
        }

        static void OnToolbarGUI()
        {
            if (sceneSelected >= m_SceneName.Length) //空项目0场景判断
                return;
            var size = EditorStyles.popup.CalcSize(new GUIContent(m_SceneName[sceneSelected]));
            // 创建水平布局
            //EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            // 将控件推到左边和右边
            //GUILayout.FlexibleSpace(); // 先占用左边的所有空间
            EditorGUILayout.LabelField("当前场景:", GUILayout.Width(55));
            int sceneSelectedNew = EditorGUILayout.Popup(sceneSelected, m_SceneName, GUILayout.Width(size.x + 5f),
                GUILayout.MinWidth(55));
            GUILayout.FlexibleSpace();
            // 结束水平布局
            //EditorGUILayout.EndHorizontal();
            if (sceneSelectedNew != sceneSelected)
            {
                sceneSelected = sceneSelectedNew;
                SceneSwitcher.PromptSaveCurrentScene();
                EditorSceneManager.OpenScene(m_ScenePath[sceneSelectedNew]);
            }
        }
    }

    static class SceneSwitcher
    {
        public static bool PromptSaveCurrentScene()
        {
            // 检查当前场景是否已保存
            if (SceneManager.GetActiveScene().isDirty)
            {
                // 提示用户是否要保存当前场景
                bool saveScene = EditorUtility.DisplayDialog(
                    "Save Current Scene",
                    "The current scene has unsaved changes. Do you want to save it?",
                    "Save",
                    "Cancel"
                );

                // 如果用户选择“保存”，则保存当前场景
                if (saveScene)
                {
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                }

                return saveScene;
            }

            // 如果场景已保存或者用户选择了“取消”，则返回 true，表示继续执行后续操作
            return true;
        }

        /// <summary>
        /// 获取项目中所有的场景文件，并以 (场景名, 场景路径) 的形式返回。
        /// </summary>
        public static List<(string sceneName, string scenePath)> GetAllScenesInProject()
        {
            List<(string sceneName, string scenePath)> scenes = new List<(string sceneName, string scenePath)>();

            // 查找所有场景文件
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            for (int i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string sceneName = $"{i + 1}_{Path.GetFileNameWithoutExtension(path)}";
                scenes.Add((sceneName, path));
            }

            return scenes;
        }
    }
}
#endif