using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Editor.UI
{
    public class ScriptGenerator
    {
        private const string Gap = "/";

        #region MVVM

        
        [MenuItem("GameObject/ScriptGenerator/GeneratorView", priority = 30)]
        public static void MemberView()
        {
            MvvmGenerateView();
        }
        
        // [MenuItem("GameObject/ScriptGenerator/GeneratorViewModel", priority = 31)]
        // public static void MemberViewModel()
        // {
        //     // MvvmGenerateViewModel();
        // }
        // [MenuItem("GameObject/ScriptGenerator/GeneratorModel", priority = 32)]
        // public static void MemberModel()
        // {
        //     // MvvmGenerateModel();
        // }

        private static void MvvmGenerateView(bool isUniTask = false)
        {
            var root = Selection.activeTransform;
            if (root != null)
            {
                StringBuilder strVar = new StringBuilder();
                StringBuilder strBind = new StringBuilder();
                bool isContainViewModel = false;
                MvvmErgodic(root, root, ref strVar, ref strBind,ref isContainViewModel);
                StringBuilder strFile = new StringBuilder();

                string className;
#if ENABLE_TEXTMESHPRO
                strFile.Append("using TMPro;\n");
#endif
                // if (isUniTask)
                // {
                //     strFile.Append("using Cysharp.Threading.Tasks;\n");
                // }

                strFile.Append("using UnityEngine;\n");
                strFile.Append("using UnityEngine.UI;\n");
                strFile.Append("using UnityFramework;\n\n");
                strFile.Append($"namespace {ScriptGeneratorSetting.GetUINameSpace()}\n");
                strFile.Append("{\n");
                
                var widgetPrefix = $"{(ScriptGeneratorSetting.GetCodeStyle() == UIFieldCodeStyle.MPrefix ? "m_" : "_")}{ScriptGeneratorSetting.GetWidgetName()}";
                if (root.name.StartsWith(widgetPrefix))
                {
                    className =  root.name.Replace(widgetPrefix, "").Replace("Widget","");
                    strFile.Append("\tclass " + root.name.Replace(widgetPrefix, "") + " : UIWidget\n");
                }
                else
                {
                    className = root.name.Replace("Window","");
                    strFile.Append("\t[Window(UILayer.UI)]\n");
                    strFile.Append("\tclass " + root.name + " : UIWindow\n");
                }
                
                strFile.Append("\t{\n");
                
                
                
                if (isContainViewModel)
                {
                    string rootCamel = char.ToLower(className[0]) + className.Substring(1);

                    strVar.Append($"\t\tprivate {className}ViewModel _{rootCamel}ViewModel;\n\n");
                }
                
                // 脚本工具生成的代码
                strFile.Append("\t\t#region 脚本工具生成的代码\n");
                strFile.Append(strVar);
                strFile.Append("\t\tprotected override void ScriptGenerator()\n");
                strFile.Append("\t\t{\n");
                strFile.Append(strBind);
                strFile.Append("\t\t}\n");
                strFile.Append("\t\t#endregion\n");

                if (isContainViewModel)
                {
                    strFile.Append("\t\tprotected override void OnCreate()\n");
                    strFile.Append("\t\t{\n");
                    strFile.Append("\t\tbase.OnCreate();\n");
                    string rootCamel = char.ToLower(className[0]) + className.Substring(1);
                    strFile.Append($"\t\t_{rootCamel}ViewModel = new {className}ViewModel();\n");
                    strFile.Append($"\t\tBindingSet<{root.name}, {className}ViewModel> bindingSet = this.CreateBindingSet(_{rootCamel}ViewModel);\n");
                    
                    MvvmBind(root, root, ref strFile);
                    strFile.Append("\t\tbindingSet.Build();\n");
                    strFile.Append("\t\t}\n");
                }

                strFile.Append("\n\n");
                strFile.Append("\t}\n");
                strFile.Append("}\n");
                
                TextEditor te = new TextEditor();
                te.text = strFile.ToString();
                te.SelectAll();
                te.Copy();
            }

            Debug.Log($"脚本已生成到剪贴板，请自行Ctl+V粘贴");
        }

        private static void MvvmBind(Transform root,Transform transform,  ref StringBuilder strFile)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                    
                string varName = child.name;
            
                string componentName = string.Empty;

                var rule = ScriptGeneratorSetting.GetScriptGenerateRule().Find(t => varName.StartsWith(t.uiElementRegex));

                if (rule != null)
                {
                    componentName = rule.componentName;
                }
                    
                if (child.name.StartsWith("m_vm"))
                {
                    string controlName = varName.Substring(1);
                    string viewModelMember = "";

                    if (componentName == "Button")
                    {
                        viewModelMember = ToPascalCase(controlName) + "Command";
                        var replace = viewModelMember.Substring(6);
                        strFile.Append($"\t\t\tbindingSet.Bind({controlName}).From(v => v.onClick).To(vm => vm.{replace});\n");
                    }
                    else if (componentName == "Text")
                    {
                        viewModelMember = ToPascalCase(controlName); // 如 Score
                        var replace = viewModelMember.Substring(7);
                        strFile.Append($"\t\t\tbindingSet.Bind({viewModelMember}).From(v => v.text).To(vm => vm.{replace}).TwoWay();\n");
                    }
                }
                    
                MvvmBind(root,child,ref strFile);
            }
        }

        private static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        public static void MvvmErgodic(Transform root, Transform transform, ref StringBuilder strVar, ref StringBuilder strBind, ref bool isContainViewModel)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                
                if (child.name.StartsWith("m_vm"))
                {
                    isContainViewModel = true;
                }

                MvvmWriteScript(root, child, ref strVar, ref strBind);
                if (child.name.StartsWith("m_item"))
                {
                    continue;
                }

                MvvmErgodic(root, child, ref strVar, ref strBind,ref isContainViewModel);
            }
        }
        
        
        private static void MvvmWriteScript(Transform root, Transform child, ref StringBuilder strVar, ref StringBuilder strBind)
        {
            string varName = child.name;
            
            string componentName = string.Empty;

            var rule = ScriptGeneratorSetting.GetScriptGenerateRule().Find(t => varName.StartsWith(t.uiElementRegex));

            if (rule != null)
            {
                componentName = rule.componentName;
            }
            
            bool isUIWidget = rule is { isUIWidget: true };

            if (componentName == string.Empty)
            {
                return;
            }
            
            var codeStyle = ScriptGeneratorSetting.Instance.CodeStyle;
            if (codeStyle == UIFieldCodeStyle.UnderscorePrefix)
            {
                if (varName.StartsWith("_"))
                {
                    
                }
                else if(varName.StartsWith("m_"))
                {
                    varName = varName.Substring(1);
                }
                else
                {
                    varName = $"_{varName}";
                }
            }
            else if (codeStyle == UIFieldCodeStyle.MPrefix)
            {
                if (varName.StartsWith("m_"))
                {
                    
                }
                else if (varName.StartsWith("_"))
                {
                    varName = $"m{varName}";
                }
                else
                {
                    varName = $"m_{varName}";
                }
            }

            string varPath = GetRelativePath(child, root);
            if (!string.IsNullOrEmpty(varName))
            {
                strVar.Append("\t\tprivate " + componentName + " " + varName + ";\n");
                switch (componentName)
                {
                    case "Transform":
                        strBind.Append($"\t\t\t{varName} = FindChild(\"{varPath}\");\n");
                        break;
                    case "GameObject":
                        strBind.Append($"\t\t\t{varName} = FindChild(\"{varPath}\").gameObject;\n");
                        break;
                    case "AnimationCurve":
                        strBind.Append($"\t\t\t{varName} = FindChildComponent<AnimCurveObject>(\"{varPath}\").m_animCurve;\n");
                        break;
                    default:
                        if (isUIWidget)
                        {
                            strBind.Append($"\t\t\t{varName} = CreateWidgetByType<{componentName}>(\"{varPath}\");\n");
                        }
                        strBind.Append($"\t\t\t{varName} = FindChildComponent<{componentName}>(\"{varPath}\");\n");
                        break;
                }
            }
        }

        #endregion
        
        
        [MenuItem("GameObject/ScriptGenerator/UIProperty", priority = 41)]
        public static void MemberProperty()
        {
            Generate(false);
        }

        [MenuItem("GameObject/ScriptGenerator/UIProperty - UniTask", priority = 43)]
        public static void MemberPropertyUniTask()
        {
            Generate(false, true);
        }

        [MenuItem("GameObject/ScriptGenerator/UIPropertyAndListener", priority = 42)]
        public static void MemberPropertyAndListener()
        {
            Generate(true);
        }

        [MenuItem("GameObject/ScriptGenerator/UIPropertyAndListener - UniTask", priority = 44)]
        public static void MemberPropertyAndListenerUniTask()
        {
            Generate(true, true);
        }

        private static void Generate(bool includeListener, bool isUniTask = false)
        {
            var root = Selection.activeTransform;
            if (root != null)
            {
                StringBuilder strVar = new StringBuilder();
                StringBuilder strBind = new StringBuilder();
                StringBuilder strOnCreate = new StringBuilder();
                StringBuilder strCallback = new StringBuilder();
                Ergodic(root, root, ref strVar, ref strBind, ref strOnCreate, ref strCallback, isUniTask);
                StringBuilder strFile = new StringBuilder();

                if (includeListener)
                {
#if ENABLE_TEXTMESHPRO
                    strFile.Append("using TMPro;\n");
#endif
                    if (isUniTask)
                    {
                        strFile.Append("using Cysharp.Threading.Tasks;\n");
                    }

                    strFile.Append("using UnityEngine;\n");
                    strFile.Append("using UnityEngine.UI;\n");
                    strFile.Append("using UnityFramework;\n\n");
                    strFile.Append($"namespace {ScriptGeneratorSetting.GetUINameSpace()}\n");
                    strFile.Append("{\n");
                    
                    var widgetPrefix = $"{(ScriptGeneratorSetting.GetCodeStyle() == UIFieldCodeStyle.MPrefix ? "m_" : "_")}{ScriptGeneratorSetting.GetWidgetName()}";
                    if (root.name.StartsWith(widgetPrefix))
                    {
                        strFile.Append("\tclass " + root.name.Replace(widgetPrefix, "") + " : UIWidget\n");
                    }
                    else
                    {
                        strFile.Append("\t[Window(UILayer.UI)]\n");
                        strFile.Append("\tclass " + root.name + " : UIWindow\n");
                    }
                    
                    strFile.Append("\t{\n");
                }

                // 脚本工具生成的代码
                strFile.Append("\t\t#region 脚本工具生成的代码\n");
                strFile.Append(strVar);
                strFile.Append("\t\tprotected override void ScriptGenerator()\n");
                strFile.Append("\t\t{\n");
                strFile.Append(strBind);
                strFile.Append(strOnCreate);
                strFile.Append("\t\t}\n");
                strFile.Append("\t\t#endregion");

                if (includeListener)
                {
                    strFile.Append("\n\n");
                    // #region 事件
                    strFile.Append("\t\t#region 事件\n");
                    strFile.Append(strCallback);
                    strFile.Append("\t\t#endregion\n\n");

                    strFile.Append("\t}\n");
                    strFile.Append("}\n");
                }

                TextEditor te = new TextEditor();
                te.text = strFile.ToString();
                te.SelectAll();
                te.Copy();
            }

            Debug.Log($"脚本已生成到剪贴板，请自行Ctl+V粘贴");
        }

        public static void Ergodic(Transform root, Transform transform, ref StringBuilder strVar, ref StringBuilder strBind, ref StringBuilder strOnCreate,
            ref StringBuilder strCallback, bool isUniTask)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                WriteScript(root, child, ref strVar, ref strBind, ref strOnCreate, ref strCallback, isUniTask);
                if (child.name.StartsWith("m_item"))
                {
                    continue;
                }

                Ergodic(root, child, ref strVar, ref strBind, ref strOnCreate, ref strCallback, isUniTask);
            }
        }

        private static string GetRelativePath(Transform child, Transform root)
        {
            StringBuilder path = new StringBuilder();
            path.Append(child.name);
            while (child.parent != null && child.parent != root)
            {
                child = child.parent;
                path.Insert(0, Gap);
                path.Insert(0, child.name);
            }

            return path.ToString();
        }

        public static string GetBtnFuncName(string varName)
        {
            var codeStyle = ScriptGeneratorSetting.Instance.CodeStyle;
            if (codeStyle == UIFieldCodeStyle.MPrefix)
            {
                return "OnClick" + varName.Replace("m_btn", string.Empty) + "Btn";
            }
            else
            {
                return "OnClick" + varName.Replace("_btn", string.Empty) + "Btn";
            }
        }

        public static string GetToggleFuncName(string varName)
        {
            var codeStyle = ScriptGeneratorSetting.Instance.CodeStyle;
            if (codeStyle == UIFieldCodeStyle.MPrefix)
            {
                return "OnToggle" + varName.Replace("m_toggle", string.Empty) + "Change";
            }
            else
            {
                return "OnToggle" + varName.Replace("_toggle", string.Empty) + "Change";
            }
        }

        public static string GetSliderFuncName(string varName)
        {
            var codeStyle = ScriptGeneratorSetting.Instance.CodeStyle;
            if (codeStyle == UIFieldCodeStyle.MPrefix)
            {
                return "OnSlider" + varName.Replace("m_slider", string.Empty) + "Change";
            }
            else
            {
                return "OnSlider" + varName.Replace("_slider", string.Empty) + "Change";
            }
        }

        private static void WriteScript(Transform root, Transform child, ref StringBuilder strVar, ref StringBuilder strBind, ref StringBuilder strOnCreate,
            ref StringBuilder strCallback, bool isUniTask)
        {
            string varName = child.name;
            
            string componentName = string.Empty;

            var rule = ScriptGeneratorSetting.GetScriptGenerateRule().Find(t => varName.StartsWith(t.uiElementRegex));

            if (rule != null)
            {
                componentName = rule.componentName;
            }
            
            bool isUIWidget = rule is { isUIWidget: true };

            if (componentName == string.Empty)
            {
                return;
            }
            
            var codeStyle = ScriptGeneratorSetting.Instance.CodeStyle;
            if (codeStyle == UIFieldCodeStyle.UnderscorePrefix)
            {
                if (varName.StartsWith("_"))
                {
                    
                }
                else if(varName.StartsWith("m_"))
                {
                    varName = varName.Substring(1);
                }
                else
                {
                    varName = $"_{varName}";
                }
            }
            else if (codeStyle == UIFieldCodeStyle.MPrefix)
            {
                if (varName.StartsWith("m_"))
                {
                    
                }
                else if (varName.StartsWith("_"))
                {
                    varName = $"m{varName}";
                }
                else
                {
                    varName = $"m_{varName}";
                }
            }

            string varPath = GetRelativePath(child, root);
            if (!string.IsNullOrEmpty(varName))
            {
                strVar.Append("\t\tprivate " + componentName + " " + varName + ";\n");
                switch (componentName)
                {
                    case "Transform":
                        strBind.Append($"\t\t\t{varName} = FindChild(\"{varPath}\");\n");
                        break;
                    case "GameObject":
                        strBind.Append($"\t\t\t{varName} = FindChild(\"{varPath}\").gameObject;\n");
                        break;
                    case "AnimationCurve":
                        strBind.Append($"\t\t\t{varName} = FindChildComponent<AnimCurveObject>(\"{varPath}\").m_animCurve;\n");
                        break;
                    default:
                        if (isUIWidget)
                        {
                            strBind.Append($"\t\t\t{varName} = CreateWidgetByType<{componentName}>(\"{varPath}\");\n");
                        }
                        strBind.Append($"\t\t\t{varName} = FindChildComponent<{componentName}>(\"{varPath}\");\n");
                        break;
                }

                if (componentName == "Button")
                {
                    string varFuncName = GetBtnFuncName(varName);
                    if (isUniTask)
                    {
                        strOnCreate.Append($"\t\t\t{varName}.onClick.AddListener(UniTask.UnityAction({varFuncName}));\n");
                        strCallback.Append($"\t\tprivate async UniTaskVoid {varFuncName}()\n");
                        strCallback.Append("\t\t{\n await UniTask.Yield();\n\t\t}\n");
                    }
                    else
                    {
                        strOnCreate.Append($"\t\t\t{varName}.onClick.AddListener({varFuncName});\n");
                        strCallback.Append($"\t\tprivate void {varFuncName}()\n");
                        strCallback.Append("\t\t{\n\t\t}\n");
                    }
                }
                else if (componentName == "Toggle")
                {
                    string varFuncName = GetToggleFuncName(varName);
                    strOnCreate.Append($"\t\t\t{varName}.onValueChanged.AddListener({varFuncName});\n");
                    strCallback.Append($"\t\tprivate void {varFuncName}(bool isOn)\n");
                    strCallback.Append("\t\t{\n\t\t}\n");
                }
                else if (componentName == "Slider")
                {
                    string varFuncName = GetSliderFuncName(varName);
                    strOnCreate.Append($"\t\t\t{varName}.onValueChanged.AddListener({varFuncName});\n");
                    strCallback.Append($"\t\tprivate void {varFuncName}(float value)\n");
                    strCallback.Append("\t\t{\n\t\t}\n");
                }
            }
        }

        public class GeneratorHelper : EditorWindow
        {
            [MenuItem("GameObject/ScriptGenerator/About", priority = 49)]
            public static void About()
            {
                GeneratorHelper welcomeWindow = (GeneratorHelper)EditorWindow.GetWindow(typeof(GeneratorHelper), false, "About");
            }

            public void Awake()
            {
                minSize = new Vector2(400, 600);
            }

            protected void OnGUI()
            {
                GUILayout.BeginVertical();
                foreach (var item in ScriptGeneratorSetting.GetScriptGenerateRule())
                {
                    GUILayout.Label(item.uiElementRegex + "：\t" + item.componentName);
                }

                GUILayout.EndVertical();
            }
        }
    }
}