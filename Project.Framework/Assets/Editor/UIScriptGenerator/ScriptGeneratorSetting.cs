using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Editor.UI
{
    public enum UIFieldCodeStyle
    {
        /// <summary>
        /// Field names start with underscore (e.g., _variable)
        /// </summary>
        [InspectorName("Field names start with underscore (e.g., _variable)")]
        UnderscorePrefix,
        
        /// <summary>
        /// Field names start with m_ prefix (e.g., m_variable)
        /// </summary>
        [InspectorName("Field names start with m_ prefix (e.g., m_variable)")]
        MPrefix,
    }
    
    [Serializable]
    public class ScriptGenerateRuler
    {
        public string uiElementRegex;
        public string componentName;
        public bool isUIWidget = false;

        public ScriptGenerateRuler(string uiElementRegex, string componentName, bool isUIWidget = false)
        {
            this.uiElementRegex = uiElementRegex;
            this.componentName = componentName;
            this.isUIWidget = isUIWidget;
        }
    }

    [CustomPropertyDrawer(typeof(ScriptGenerateRuler))]
    public class ScriptGenerateRulerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var uiElementRegexRect = new Rect(position.x, position.y, 120, position.height);
            var componentNameRect = new Rect(position.x + 125, position.y, 150, position.height);
            var isUIWidgetRect = new Rect(position.x + 325, position.y, 150, position.height);
            EditorGUI.PropertyField(uiElementRegexRect, property.FindPropertyRelative("uiElementRegex"), GUIContent.none);
            EditorGUI.PropertyField(componentNameRect, property.FindPropertyRelative("componentName"), GUIContent.none);
            EditorGUI.PropertyField(isUIWidgetRect, property.FindPropertyRelative("isUIWidget"), GUIContent.none);
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    [CreateAssetMenu(menuName = "UnityFramework/ScriptGeneratorSetting", fileName = "ScriptGeneratorSetting")]
    public class ScriptGeneratorSetting : ScriptableObject
    {
        private static ScriptGeneratorSetting _instance;

        public static ScriptGeneratorSetting Instance
        {
            get
            {
                if (_instance == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:ScriptGeneratorSetting");
                    if (guids.Length >= 1)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        _instance = AssetDatabase.LoadAssetAtPath<ScriptGeneratorSetting>(path);
                    }
                }
                return _instance;
            }
        }

        // [FolderPath]
        // [LabelText("默认组件代码保存路径")]
        [SerializeField]
        private string _codePath;

        // [LabelText("绑定代码命名空间")]
        [SerializeField]
        private string _namespace = "GameLogic";

        // [LabelText("子组件名称(不会往下继续遍历)")]
        [SerializeField]
        private string _widgetName = "item";

        public string CodePath => _codePath;

        public string Namespace => _namespace;

        public string WidgetName => _widgetName;
        
        public UIFieldCodeStyle CodeStyle = UIFieldCodeStyle.UnderscorePrefix;

        [SerializeField]
        private List<ScriptGenerateRuler> scriptGenerateRule = new List<ScriptGenerateRuler>()
        {
            new ScriptGenerateRuler("m_go", "GameObject"),
            new ScriptGenerateRuler("m_item", "GameObject"),
            new ScriptGenerateRuler("m_tf", "Transform"),
            new ScriptGenerateRuler("m_rect", "RectTransform"),
            new ScriptGenerateRuler("m_text", "Text"),
            new ScriptGenerateRuler("m_richText", "RichTextItem"),
            new ScriptGenerateRuler("m_btn", "Button"),
            new ScriptGenerateRuler("m_img", "Image"),
            new ScriptGenerateRuler("m_rimg", "RawImage"),
            new ScriptGenerateRuler("m_scrollBar", "Scrollbar"),
            new ScriptGenerateRuler("m_scroll", "ScrollRect"),
            new ScriptGenerateRuler("m_input", "InputField"),
            new ScriptGenerateRuler("m_grid", "GridLayoutGroup"),
            new ScriptGenerateRuler("m_hlay", "HorizontalLayoutGroup"),
            new ScriptGenerateRuler("m_vlay", "VerticalLayoutGroup"),
            new ScriptGenerateRuler("m_slider", "Slider"),
            new ScriptGenerateRuler("m_group", "ToggleGroup"),
            new ScriptGenerateRuler("m_curve", "AnimationCurve"),
            new ScriptGenerateRuler("m_canvasGroup", "CanvasGroup"),
            new ScriptGenerateRuler("m_tmp","TextMeshProUGUI"),
        };

        public List<ScriptGenerateRuler> ScriptGenerateRule => scriptGenerateRule;


        [MenuItem("UnityFramework/Create ScriptGeneratorSetting")]
        private static void CreateAutoBindGlobalSetting()
        {
            string[] paths = AssetDatabase.FindAssets("t:ScriptGeneratorSetting");
            if (paths.Length >= 1)
            {
                string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                EditorUtility.DisplayDialog("警告", $"已存在ScriptGeneratorSetting，路径:{path}", "确认");
                return;
            }

            ScriptGeneratorSetting setting = CreateInstance<ScriptGeneratorSetting>();
            AssetDatabase.CreateAsset(setting, "Assets/Editor/ScriptGeneratorSetting.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static List<ScriptGenerateRuler> GetScriptGenerateRule()
        {
            if (Instance == null)
            {
                return null;
            }
            return Instance.ScriptGenerateRule;
        }

        public static string GetUINameSpace()
        {
            if (Instance == null)
            {
                return string.Empty;
            }

            return Instance.Namespace;
        }

        public static UIFieldCodeStyle GetCodeStyle()
        {
            if (Instance == null)
            {
                return UIFieldCodeStyle.UnderscorePrefix;
            }

            return Instance.CodeStyle;
        }

        public static string GetCodePath()
        {
            if (Instance == null)
            {
                return string.Empty;
            }

            return Instance.CodePath;
        }
        
        public static string GetWidgetName()
        {
            if (Instance == null)
            {
                return string.Empty;
            }

            return Instance.WidgetName;
        }
    }
}