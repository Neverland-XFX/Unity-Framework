namespace UnityFramework.Editor
{
    #if UNITY_EDITOR
    using System;
    using UnityEditor;
    using UnityEngine;

    public class AtlasConfigWindow : EditorWindow
    {
        [MenuItem("Tools/图集工具/配置面板")]
        public static void ShowWindow()
        {
            GetWindow<AtlasConfigWindow>("Atlas Config").minSize = new Vector2(450, 400);
        }

        private Vector2 _scrollPosition;

        private int[] _paddingEnum = new int[] { 2, 4, 8 };
        private bool _showExcludeKeywords = false; // 新增折叠状态变量

        private void OnGUI()
        {
            var config = AtlasConfiguration.Instance;

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;

                EditorGUI.BeginChangeCheck();

                DrawFolderSettings(config);
                DrawPlatformSettings(config);
                DrawPackingSettings(config);
                DrawAdvancedSettings(config);

                if (EditorGUI.EndChangeCheck())
                {
                    AtlasConfiguration.Save(true);
                    AssetDatabase.Refresh();
                }

                DrawActionButtons();
            }
        }

        private void DrawFolderSettings(AtlasConfiguration config)
        {
            GUILayout.Label("目录设置", EditorStyles.boldLabel);
            config.outputAtlasDir = DrawFolderField("输出目录", config.outputAtlasDir);
            config.sourceAtlasRoot = DrawFolderField("收集目录", config.sourceAtlasRoot);
            config.excludeFolder = DrawFolderField("排除目录", config.excludeFolder);
            EditorGUILayout.Space();
        }

        private string DrawFolderField(string label, string path)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                path = EditorGUILayout.TextField(label, path);
                if (GUILayout.Button("选择", GUILayout.Width(60)))
                {
                    var newPath = EditorUtility.OpenFolderPanel(label, Application.dataPath, "");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        path = "Assets" + newPath.Substring(Application.dataPath.Length);
                    }
                }
            }

            return path;
        }

        private void DrawPlatformSettings(AtlasConfiguration config)
        {
            GUILayout.Label("平台设置", EditorStyles.boldLabel);
            config.androidFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Android 格式", config.androidFormat);
            config.iosFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("iOS 格式", config.iosFormat);
            config.webglFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("WebGL 格式", config.webglFormat);
            config.compressionQuality = EditorGUILayout.IntSlider("压缩质量", config.compressionQuality, 0, 100);
            EditorGUILayout.Space();
        }

        private void DrawPackingSettings(AtlasConfiguration config)
        {
            GUILayout.Label("PackingSetting", EditorStyles.boldLabel);
            config.padding = EditorGUILayout.IntPopup("Padding",
                config.padding,
                Array.ConvertAll(_paddingEnum, x => x.ToString()),
                _paddingEnum);
            config.blockOffset = EditorGUILayout.IntField("BlockOffset", config.blockOffset);
            config.enableRotation = EditorGUILayout.Toggle("Enable Rotation", config.enableRotation);
            config.tightPacking = EditorGUILayout.Toggle("剔除透明区域", config.tightPacking);
            EditorGUILayout.Space();
        }


        private void DrawAdvancedSettings(AtlasConfiguration config)
        {
            GUILayout.Label("高级设置", EditorStyles.boldLabel);
            config.autoGenerate = EditorGUILayout.Toggle("自动生成", config.autoGenerate);
            config.enableLogging = EditorGUILayout.Toggle("启用日志", config.enableLogging);
            config.enableV2 = EditorGUILayout.Toggle("启用V2打包", config.enableV2);
            // 优化后的排除关键词显示
            _showExcludeKeywords = EditorGUILayout.BeginFoldoutHeaderGroup(_showExcludeKeywords, "排除关键词");
            if (_showExcludeKeywords)
            {
                int keywordCount = config.excludeKeywords?.Length ?? 0;
                int newCount = EditorGUILayout.IntField("数量", keywordCount);

                // 调整数组大小
                if (newCount != keywordCount)
                {
                    Array.Resize(ref config.excludeKeywords, newCount);
                }

                // 绘制每个元素
                for (int i = 0; i < newCount; i++)
                {
                    config.excludeKeywords[i] = EditorGUILayout.TextField($"关键词 {i + 1}",
                        config.excludeKeywords[i] ?? "");
                }

                // 添加快捷按钮
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("添加", GUILayout.Width(60)))
                {
                    Array.Resize(ref config.excludeKeywords, newCount + 1);
                }

                if (GUILayout.Button("清空", GUILayout.Width(60)))
                {
                    config.excludeKeywords = Array.Empty<string>();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();
        }

        private void DrawActionButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("立即生成所有图集", GUILayout.Height(30)))
                {
                    EditorSpriteSaveInfo.ForceGenerateAll();
                }

                if (GUILayout.Button("清空缓存", GUILayout.Height(30)))
                {
                    EditorSpriteSaveInfo.ClearCache();
                }
            }
        }
    }

#endif
}