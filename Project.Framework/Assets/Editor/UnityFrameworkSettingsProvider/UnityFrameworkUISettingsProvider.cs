using UnityFramework.Editor.UI;
using UnityEditor;  

public static class UnityFrameworkUISettingsProvider  
{  
    [MenuItem("UnityFramework/Settings/UnityFrameworkUISettings", priority = -1)]
    public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/UnityFramework/UISettings");
    
    private const string SettingsPath = "Project/UnityFramework/UISettings";  

    [SettingsProvider]  
    public static SettingsProvider CreateMySettingsProvider()  
    {  
        return new SettingsProvider(SettingsPath, SettingsScope.Project)  
        {  
            label = "UnityFramework/UISettings",  
            guiHandler = (searchContext) =>  
            {  
                var scriptGeneratorSetting = ScriptGeneratorSetting.Instance;  
                var scriptGenerator = new SerializedObject(scriptGeneratorSetting);  

                EditorGUILayout.PropertyField(scriptGenerator.FindProperty("_codePath"));  
                EditorGUILayout.PropertyField(scriptGenerator.FindProperty("_namespace"));  
                EditorGUILayout.PropertyField(scriptGenerator.FindProperty("_widgetName"));  
                EditorGUILayout.PropertyField(scriptGenerator.FindProperty("CodeStyle"));  
                EditorGUILayout.PropertyField(scriptGenerator.FindProperty("scriptGenerateRule"));  
                scriptGenerator.ApplyModifiedProperties();
            },  
            keywords = new[] { "UnityFramework", "Settings", "Custom" }  
        };  
    }
}  
